using Godot;

public partial class SpawnerDataResource : SpawnResource {
    public enum SpawnTrigger {
        OnSeen,
        OnPlaced,
        Event,
        PlayerSpawnEvent
    }

    public enum DespawnCondition {
        None,
        AllSpawned,
        RequireKill
    }

    [Export]
    public SpawnTrigger trigger = SpawnTrigger.OnSeen;
    [Export]
    public SpawnResource spawn = null;
    /// <summary>
    /// All points this spawner will spawn on
    /// </summary>
    [Export]
    public Vector2[] spawnPoints = new Vector2[1];
    [Export]
    public float startRotation = 0f;
    [Export]
    public float rotationIncrement = 0f;
    [Export]
    public bool moveWithSpawner = false;
    /// <summary>
    /// How long it will take to spawn all items
    /// </summary>
    [Export]
    public double time = 0f;
    [Export]
    public DespawnCondition despawnCondition = DespawnCondition.AllSpawned;

    public double TimePerSpawn {
        get => time / spawnPoints.Length;
    }

    public override Script GetDefaultScript () {
        return STGScripts.scripts[nameof (Spawner)];
    }
}