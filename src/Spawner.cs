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
    
    public static Dictionary<NodePath, SpawnerSaveData> States { get; private set; } = new Dictionary<NodePath, SpawnerSaveData> ();

    public override void _Ready () {
        if (!STGController.Instance.IsTracked (this)) {
            STGController.Instance.RequestTrack (this);
            EmitSignal ("Spawn");
        }
    }

    public override void _EnterTree () {
        base._EnterTree ();
        Trigger += _OnTrigger;
        STGController.Instance.PlayerSpawn += _OnPlayerSpawnEvent;
        STGController.Instance.SaveCheckpoint += SaveState;
        STGController.Instance.LoadCheckpoint += LoadState;
    }

    public override void _ExitTree () {
        base._ExitTree ();
        Trigger -= _OnTrigger;
        STGController.Instance.PlayerSpawn -= _OnPlayerSpawnEvent;
        STGController.Instance.SaveCheckpoint -= SaveState;
        STGController.Instance.LoadCheckpoint -= LoadState;
    }

    public override void _PhysicsProcess (double delta) {
        if (fireId >= 0 && fireId < spawnData.spawnOffsetPoints.Length) {
            double timeSinceFire = timeElapsed - timeTrigger;
            timeSinceFire -= spawnData.TimePerSpawn * fireId;   // Stay up-to-date with our existing spawns
            while (fireId < spawnData.spawnOffsetPoints.Length && timeSinceFire >= spawnData.TimePerSpawn) {
                Spawnable spawn = STGController.Instance.Spawn (spawnData.spawn, Position + spawnData.spawnOffsetPoints[fireId], GetPath ());
                spawn.RotationDegrees = spawnData.startRotation + spawnData.rotationIncrement * fireId;
                
                spawns.Add (spawn);
                spawn.Despawn += UpdateTrackedSpawns;

                fireId++;
                timeSinceFire -= spawnData.TimePerSpawn;
            }
            if (spawnData.despawnCondition == SpawnerDataResource.DespawnCondition.AllSpawned
                && fireId == spawnData.spawnOffsetPoints.Length)
                STGController.Instance.Despawn (this);
        }

        base._PhysicsProcess (delta);
    }

    private void FireSpawn () {
        timeTrigger = timeElapsed;
        fireId = 0;
    }

    protected void UpdateTrackedSpawns () {
        Spawnable spawn = spawns.First (s => s.Active == false);
        spawn.Despawn -= UpdateTrackedSpawns;
        spawns.Remove (spawn);

        if (spawnData.despawnCondition == SpawnerDataResource.DespawnCondition.RequireKill
            && spawns.Count == 0)
            STGController.Instance.Despawn (this);
    }

    public override void _OnSeen () {
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.OnSeen)
            FireSpawn ();
    }

    public virtual void _OnTrigger () {
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.Event)
            FireSpawn ();
    }

    public override void _OnSpawn () {
        foreach (Spawnable spawn in spawns)
            spawn.Despawn -= UpdateTrackedSpawns;
        spawns.Clear ();

        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.OnPlaced)
            FireSpawn ();
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
    public delegate void TriggerEventHandler ();
}