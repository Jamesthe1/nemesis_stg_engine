using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class STGController : Node2D {
    public enum StageLoadProcess {
        Nothing,
        UseCheckpoint,
        ClearStats
    }

    protected List<Spawnable> active = new List<Spawnable> ();
    protected List<Spawnable> spare = new List<Spawnable> ();

    public static List<PlayerEntity> Players { get; private set; } = new List<PlayerEntity> ();
    protected List<Spawner> PlayerSpawners {
        get => active.OfType<Spawner> ()
                     .Where (s => s.spawnData.trigger == SpawnerDataResource.SpawnTrigger.PlayerSpawnEvent)
                     .ToList ();
    }

    public static Dictionary<int, PlayerEntity> Devices {
        get {
            int[] devices = Input.GetConnectedJoypads ().ToArray ();
            return devices.ToDictionary (d => d, d => Players.FirstOrDefault (p => p.DeviceID == d));
        }
    }

    public static List<Entity> Bosses {
        get => Instance.active.OfType<Entity> ()
                              .Where (e => e.entityData.isBoss)
                              .ToList ();
    }

    [Export]
    public Vector2 stageMovement = Vector2.Zero;
    [Export]
    public NodePath parallaxBgPath = "";

    public Vector2 StagePos { get; protected set; } = Vector2.Zero;
    public List<Node2D> movables = new List<Node2D> ();

    public static STGController Instance {
        get; private set;
    }

    private static int _score = 0;
    public static int Score {
        get => _score;
        set {
            _score = value;
            Instance.EmitSignal ("ScoreUpdate");
        }
    }

    protected static NodePath checkpoint = "";

    public override void _EnterTree () {
        Instance = this;
    }

    public override void _Ready () {
        EmitSignal ("PlayerSpawn");
        movables.AddRange (Players);
        movables.AddRange (PlayerSpawners);
        movables.AddRange (GetChildren ().OfType<Camera2D> ());
        EmitSignal ("StageStart");
    }

    public virtual void ClearStats () {
        checkpoint = "";
        Score = 0;
    }

    public void MoveStageTo (Vector2 pos, bool moveAllSpawnables = false) {
        List<Node2D> m_movables = new List<Node2D> (movables);  // Clones the movables list so we don't accidentally modify the original
        if (moveAllSpawnables) {
            m_movables.AddRange (
                GetChildren ()
                    .OfType<Spawnable> ()
                    .Where (s => !movables.Contains (s))        // Avoid duplicate entries
            );
        }

        Vector2 shift = pos - StagePos;

        if (GetParent ().Name != "root") {
            shift *= 0.5f;
            Position -= shift;
        }
        foreach (Node2D movable in m_movables)
            movable.Position += shift;   // Keep them on-screen
        StagePos += shift;
    }

    public void MoveStageTo (NodePath nodePath, Node root, bool moveAllSpawnables = false) {
        MoveStageTo (root.GetNode<Node2D> (nodePath).Position, moveAllSpawnables);
    }

    public override void _PhysicsProcess (double delta) {
        if (Engine.IsEditorHint ())
            return;

        Vector2 moveStep = stageMovement * (float)delta;
        MoveStageTo (StagePos + moveStep);
        if (parallaxBgPath != "")
            GetNode<Node2D> (parallaxBgPath).Position -= moveStep;  // Keep illusion of smooth movement by decoupling bg movement from world
    }

    /// <summary>
    /// Spawns a spawnable using the provided resource details
    /// </summary>
    /// <returns>The new object that was spawned</returns>
    public Spawnable Spawn (SpawnResource resource, Vector2 pos, NodePath spawnerPath) {
        if (resource == null)
            throw new ArgumentNullException ($"{nameof(SpawnResource)} cannot be null");

        bool hadToSpawn = false;
        Spawnable spawnable = spare.FirstOrDefault (s => s.Data.GetRelatedScript ().Equals (resource.GetRelatedScript ()));
        if (spawnable == null) {
            hadToSpawn = true;
            spawnable = resource.GetRelatedScript ().Instantiate<CharacterBody2D> (resource.name) as Spawnable;
        }
        else
            spare.Remove (spawnable);
        active.Add (spawnable);

        spawnable.Data = resource;
        spawnable.spawnerPath = spawnerPath;

        // Moving this after adding to active due to _EnterTree event
        if (hadToSpawn)
            AddChild (spawnable);
        else
            foreach (Node child in spawnable.GetChildren ())
                child.Free ();

        foreach (Node2D child in spawnable.ConstructChildren ())
            spawnable.AddChild (child);

        spawnable.Position = pos;
        spawnable.Rotation = GetNode<Node2D> (spawnerPath).Rotation;

        spawnable.Active = true;    // Also sets collision mask and layer
        spawnable.EmitSignal ("Spawned");
        return spawnable;
    }

    /// <summary>
    /// Checks whether or not the spawnable is being handled by the STG controller
    /// </summary>
    /// <returns>A state whether or not this node's spawn behavior is being handled by the STG controller</returns>
    public bool IsTracked (Spawnable spawnable) {
        return active.Contains (spawnable) || spare.Contains (spawnable);
    }

    /// <summary>
    /// Requests to manage the handling of a spawnable
    /// </summary>
    /// <returns>A state whether or not the request could be fulfilled</returns>
    public bool RequestTrack (Spawnable spawnable) {
        if (IsTracked (spawnable))
            return false;
        if (spawnable.Active)
            active.Add (spawnable);
        else
            spare.Add (spawnable);
        return true;
    }

    /// <summary>
    /// Despawns a spawnable
    /// </summary>
    /// <returns>A state whether or not the object could be despawned</returns>
    public bool Despawn (Spawnable spawnable) {
        if (!active.Contains (spawnable))
            return false;
        active.Remove (spawnable);
        spare.Add (spawnable);

        spawnable.Active = false;
        spawnable.EmitSignal ("Despawned");
        return true;
    }

    public static void SetNewCheckpoint (NodePath newCheckpoint) {
        checkpoint = newCheckpoint;
        Instance.EmitSignal ("SaveCheckpoint");
    }

    public static void UnloadStage () {
        Instance.EmitSignal ("StageStartUnload");
        Instance.Free ();
    }

    public static STGController LoadStage (PackedScene scene, Node parent, StageLoadProcess process = StageLoadProcess.ClearStats) {
        if (parent == null)
            throw new ArgumentNullException ("Parent cannot be null");
        
        STGController stage = scene.Instantiate<STGController> ();
        parent.AddChild (stage);

        if (process == StageLoadProcess.UseCheckpoint) {
            stage.MoveStageTo (stage.GetNode<Node2D> (checkpoint).Position);
            stage.EmitSignal ("LoadCheckpoint");
        }
        else if (process == StageLoadProcess.ClearStats)
            stage.ClearStats ();

        stage.RequestReady ();
        return stage;
    }

    public static STGController ReloadStage (StageLoadProcess process = StageLoadProcess.UseCheckpoint) {
        PackedScene scene = GD.Load<PackedScene> (Instance.SceneFilePath);
        Node parent = Instance.GetParent ();
        UnloadStage ();
        return LoadStage (scene, parent, process);
    }

    [Signal]
    public delegate void PlayerSpawnEventHandler ();
    [Signal]
    public delegate void StageStartEventHandler ();
    [Signal]
    public delegate void StageEndEventHandler ();
    [Signal]
    public delegate void ScoreUpdateEventHandler ();

    [Signal]
    public delegate void BossAlarmEventHandler ();
    [Signal]
    public delegate void BossSpawnedEventHandler (Entity boss);
    [Signal]
    public delegate void BossDestroyedEventHandler (Entity boss);

    [Signal]
    public delegate void SaveCheckpointEventHandler ();
    [Signal]
    public delegate void LoadCheckpointEventHandler ();
    [Signal]
    public delegate void StageStartUnloadEventHandler ();
}