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

    private List<Spawnable> active = new List<Spawnable> ();
    private List<Spawnable> spare = new List<Spawnable> ();

    public static List<PlayerEntity> Players { get; private set; }
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

    [Export]
    public Vector2 stageMovement = Vector2.Zero;
    [Export]
    public NodePath parallaxBgPath = "";

    public static STGController Instance {
        get; private set;
    }
    protected static NodePath checkpoint = "";

    public override void _EnterTree () {
        Instance = this;
        Players = new List<PlayerEntity> ();
    }

    public override void _Ready () {
        EmitSignal ("PlayerSpawn");
    }

    public virtual void ClearStats () {
        checkpoint = "";
    }

    public void MoveStageTo (Vector2 pos) {
        Vector2 shift = pos - Position;
        List<Node2D> movables = Players.Cast<Node2D> ().ToList ();
        movables.AddRange (PlayerSpawners);
        movables.AddRange (GetChildren ().OfType<Camera2D> ());

        if (GetParent ().Name != "root") {
            shift *= 0.5f;
            Position -= shift;
        }
        foreach (Node2D movable in movables)
            movable.Position += shift;   // Keep them on-screen
    }

    public void MoveStageTo (NodePath nodePath) {
        MoveStageTo (GetNode<Node2D> (nodePath).Position);
    }

    public override void _PhysicsProcess (double delta) {
        if (Engine.IsEditorHint ())
            return;

        Vector2 moveStep = stageMovement * (float)delta;
        MoveStageTo (Position + moveStep);
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
        Spawnable spawnable = spare.FirstOrDefault (s => s.Data.baseScene.Equals (resource.baseScene));
        if (spawnable == null) {
            hadToSpawn = true;
            spawnable = resource.baseScene.Instantiate () as Spawnable;
        }
        else
            spare.Remove (spawnable);
        active.Add (spawnable);

        // Moving this after adding to active due to _EnterTree event
        if (hadToSpawn)
            AddChild (spawnable);

        spawnable.SetChildIfExist ("Sprite", "texture", resource.texture);
        spawnable.SetChildIfExist ("Collision", "shape", resource.collisionShape);
        if (spawnable.HasNode ("VisCheck")) {
            Vector2 size = Vector2.One * 20;
            if (resource.texture != null)
                size = resource.texture.GetSize ();
            spawnable.SetChildIfExist ("VisCheck", "rect", size.GetCenteredRegion ());
        }

        spawnable.Data = resource;
        spawnable.Active = true;    // Also sets collision mask and layer
        spawnable.Position = pos;
        spawnable.spawnerPath = spawnerPath;

        spawnable.EmitSignal ("Spawn");
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
        spawnable.EmitSignal ("Despawn");
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

    public static void LoadStage (PackedScene scene, Node parent, StageLoadProcess process = StageLoadProcess.ClearStats) {
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
    }

    public static void ReloadStage (StageLoadProcess process = StageLoadProcess.UseCheckpoint) {
        PackedScene scene = GD.Load<PackedScene> (Instance.SceneFilePath);
        Node parent = Instance.GetParent ();
        UnloadStage ();
        LoadStage (scene, parent, process);
    }

    [Signal]
    public delegate void PlayerSpawnEventHandler ();
    [Signal]
    public delegate void BossAlarmEventHandler ();

    [Signal]
    public delegate void SaveCheckpointEventHandler ();
    [Signal]
    public delegate void LoadCheckpointEventHandler ();
    [Signal]
    public delegate void StageStartUnloadEventHandler ();
}