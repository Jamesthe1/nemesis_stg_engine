using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Spawner : Spawnable, ISaveState<SpawnerSaveData> {
    [Export]
    public SpawnerDataResource spawnData;

    public override SpawnResource Data { get => spawnData; set => spawnData = value as SpawnerDataResource; }

    protected double timeTrigger = 0.0;
    protected int fireId = -1;

    protected List<Spawnable> spawns = new List<Spawnable> ();
    protected int unkilledByPlayer = 0;
    
    public static Dictionary<NodePath, SpawnerSaveData> States { get; private set; } = new Dictionary<NodePath, SpawnerSaveData> ();

    public override void _Ready () {
        if (!STGController.Instance.IsTracked (this)) {
            STGController.Instance.RequestTrack (this);
            foreach (Node2D child in ConstructChildren ())
                AddChild (child);
            EmitSignal ("Spawned");
        }
    }

    public override void _EnterTree () {
        base._EnterTree ();
        STGController.Instance.PlayerSpawn += _OnPlayerSpawnEvent;
        STGController.Instance.SaveCheckpoint += SaveState;
        STGController.Instance.LoadCheckpoint += LoadState;
    }

    public override void _ExitTree () {
        base._ExitTree ();
        STGController.Instance.PlayerSpawn -= _OnPlayerSpawnEvent;
        STGController.Instance.SaveCheckpoint -= SaveState;
        STGController.Instance.LoadCheckpoint -= LoadState;
    }

    protected virtual void DoSpawnStep (double timeSinceFire) {
        if (spawnData.moveWithSpawner)
            Position = GetSpawnerPos ();

        while (fireId < spawnData.spawnPoints.Length && timeSinceFire >= spawnData.TimePerSpawn) {
            Spawnable spawn = STGController.Instance.Spawn (spawnData.spawn, Position + spawnData.spawnPoints[fireId], GetPath ());
            spawn.RotationDegrees = spawnData.startRotation + spawnData.rotationIncrement * fireId;
            
            spawns.Add (spawn);
            spawn.Despawned += UpdateTrackedSpawns;
            if (spawnData.despawnCondition == SpawnerDataResource.DespawnCondition.RequireKill
                && spawn is Entity entity) {
                entity.Destroyed += UpdateAlive;
                unkilledByPlayer++;
            }

            fireId++;
            timeSinceFire -= spawnData.TimePerSpawn;
        }
        if (spawnData.despawnCondition == SpawnerDataResource.DespawnCondition.AllSpawned
            && fireId == spawnData.spawnPoints.Length)
            STGController.Instance.Despawn (this);
    }

    public override void _PhysicsProcess (double delta) {
        if (!Active)
            return;

        base._PhysicsProcess (delta);

        if (fireId >= 0 && fireId < spawnData.spawnPoints.Length) {
            double timeSinceFire = timeElapsed - timeTrigger;
            timeSinceFire -= spawnData.TimePerSpawn * fireId;   // Stay up-to-date with our existing spawns
            DoSpawnStep (timeSinceFire);
        }
    }

    private void FireSpawn () {
        timeTrigger = timeElapsed;
        fireId = 0;
        DoSpawnStep (spawnData.TimePerSpawn);
        EmitSignal ("Triggered");
    }

    protected void UpdateAlive (bool destroyedByPlayer) {
        Entity entity = spawns.OfType<Entity> ().FirstOrDefault (s => s.currentHp <= 0);
        if (entity == null) {
            GD.PrintErr ($"Couldn't find a dead entity; spawns: {spawns.Count}");
            return;
        }

        entity.Destroyed -= UpdateAlive;
        entity.Despawned -= UpdateTrackedSpawns;
        spawns.Remove (entity);

        if (destroyedByPlayer)
            unkilledByPlayer--;
    }

    protected void UpdateTrackedSpawns () {
        Spawnable spawn = spawns.FirstOrDefault (s => s.Active == false);
        if (spawn == null) {
            GD.PrintErr ($"Couldn't find an inactive spawn; spawns: {spawns.Count}");
            return;
        }

        spawn.Despawned -= UpdateTrackedSpawns;
        spawns.Remove (spawn);

        if (spawnData.despawnCondition == SpawnerDataResource.DespawnCondition.RequireKill
            && spawns.Count == 0)
            STGController.Instance.Despawn (this);
    }

    public override void _OnSeen () {
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.OnSeen)
            FireSpawn ();
    }

    public virtual void _OnEvent () {
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.Event)
            FireSpawn ();
    }

    public override void _OnSpawn () {
        base._OnSpawn ();
        foreach (Spawnable spawn in spawns) {
            spawn.Despawned -= UpdateTrackedSpawns;
            if (spawn is Entity entity)
                entity.Destroyed -= UpdateAlive;
        }
        spawns.Clear ();
        unkilledByPlayer = 0;

        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.OnPlaced)
            FireSpawn ();
    }

    public override void _OnDespawn () {
        if (spawnData.despawnCondition != SpawnerDataResource.DespawnCondition.RequireKill
            || unkilledByPlayer == 0)
            base._OnDespawn ();
    }

    public virtual void _OnPlayerSpawnEvent () {
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.PlayerSpawnEvent)
            FireSpawn ();
    }

    public void SaveState () {
        States[GetPath ()] = new SpawnerSaveData (spawnData, timeTrigger, fireId);
    }

    public void LoadState () {
        SpawnerSaveData state = States[GetPath ()];
        spawnData = state.data;
        timeTrigger = state.timeTrigger;
        fireId = state.fireId;
    }

    [Signal]
    public delegate void TriggeredEventHandler ();
}