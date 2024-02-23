using Godot;

public abstract partial class SpawnResource : Resource {
    [Export]
    public string name = "Node";
    [Export]
    public Texture2D texture;
    [Export]
    public bool fixTexRotation = false;

    [Export]
    public Shape2D collisionShape;
    [Export]
    public uint collisionLayer = 1;
    [Export]
    public uint collisionMask = 1;

    [Export]
    public Script baseScript;

    /// <summary>
    /// Spawned on despawn
    /// </summary>
    [Export]
    public SpawnResource despawnSpawn;
    [Export]
    public SpawnResource intervalSpawn;
    /// <summary>
    /// The interval to spawn <see cref="intervalSpawn"/>
    /// </summary>
    [Export]
    public float interval = 1f;
}