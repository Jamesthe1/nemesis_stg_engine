using Godot;

public partial class SpawnerDataResource : SpawnResource {
    public enum SpawnTrigger {
        OnSeen,
        OnPlaced,
        Event
    }

    [Export]
    public SpawnTrigger trigger = SpawnTrigger.OnSeen;
    [Export]
    public SpawnResource[] spawns = new SpawnResource[0];
    [Export]
    public float startRotation = 0f;
    [Export]
    public float rotationIncrement = 0f;
    /// <summary>
    /// How long it will take to spawn all items
    /// </summary>
    [Export]
    public double time = 0f;
    [Export]
    public bool despawnAfter = true;
    [Export]
    public bool requireKill = false;

    public double TimePerSpawn {
        get => time / spawns.Length;
    }
}