using Godot;

public abstract partial class SpawnResource : Resource {
    [Export]
    public Texture2D texture;
    [Export]
    public Shape2D collisionShape;
    [Export]
    public uint collisionLayer = 1;
    [Export]
    public uint collisionMask = 1;
    [Export]
    public PackedScene baseScene;

    [Export]
    public SpawnResource despawnSpawn;
    [Export]
    public SpawnResource intervalSpawn;
    [Export]
    public float interval = 1f;
}