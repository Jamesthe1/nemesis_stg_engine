using System.Collections.Generic;
using Godot;

public partial class Spawner : Spawnable, ISaveState<SpawnerSaveData> {
    [Export]
    public SpawnerDataResource spawnData;

    public override SpawnResource Data { get => spawnData; set => spawnData = value as SpawnerDataResource; }

    private double timeTrigger = 0.0;
    private int fireId = -1;
    
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
        if (fireId >= 0 && fireId < spawnData.spawns.Length) {
            double timeSinceFire = timeElapsed - timeTrigger;
            timeSinceFire -= spawnData.TimePerSpawn * fireId;   // Stay up-to-date with our existing spawns
            while (fireId < spawnData.spawns.Length && timeSinceFire >= spawnData.TimePerSpawn) {
                Spawnable spawn = STGController.Instance.Spawn (spawnData.spawns[fireId], Position);
                spawn.RotationDegrees = spawnData.startRotation + spawnData.rotationIncrement * fireId;

                fireId++;
                timeSinceFire -= spawnData.TimePerSpawn;
            }
            if (spawnData.despawnAfter)
                STGController.Instance.Despawn (this);
        }

        base._PhysicsProcess (delta);
    }

    private void FireSpawn () {
        timeTrigger = timeElapsed;
        fireId = 0;
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

    public void LoadState() {
        SpawnerSaveData state = States[GetPath ()];
        spawnData = state.data;
        timeTrigger = state.timeTrigger;
        fireId = state.fireId;
    }

    [Signal]
    public delegate void TriggerEventHandler ();
}