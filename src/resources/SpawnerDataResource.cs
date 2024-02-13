using Godot;

public partial class SpawnerDataResource : SpawnResource {
    public enum SpawnTrigger {
        OnSeen,
        OnPlaced,
        Event,
        PlayerSpawnEvent
    }

    [Export]
    public SpawnTrigger trigger = SpawnTrigger.OnSeen;
    [Export]
    public SpawnResource spawn = null;
    /// <summary>
    /// All points this spawner will spawn on
    /// </summary>
    [Export]
    public Vector2[] spawnOffsetPoints = new Vector2[1];
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
        get => time / spawnOffsetPoints.Length;
    }
}