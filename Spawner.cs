using Godot;

public partial class Spawner : Spawnable {
    [Export]
    public SpawnerDataResource spawnData;

    public override SpawnResource Data { get => spawnData; set => spawnData = value as SpawnerDataResource; }

    public override void _Ready () {
        if (!STGController.Instance.IsTracked (this)) {
            STGController.Instance.RequestTrack (this);
            EmitSignal ("Spawn");
        }
    }

    public override void _EnterTree () {
        base._EnterTree ();
        Trigger += _OnTrigger;
    }

    public override void _ExitTree () {
        base._ExitTree ();
        Trigger -= _OnTrigger;
    }

    private void FireSpawn () {
        foreach (var locations in spawnData.spawnPoints) {
            foreach (var spawn in locations.Value) {
                float rad = Mathf.DegToRad (spawn.Value);
                STGController.Instance.Spawn (spawn.Key, locations.Key).Rotate (rad);
            }
        }
        if (spawnData.despawnAfter)
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
        if (spawnData.trigger == SpawnerDataResource.SpawnTrigger.OnAppear)
            FireSpawn ();
    }

    [Signal]
    public delegate void TriggerEventHandler ();
}