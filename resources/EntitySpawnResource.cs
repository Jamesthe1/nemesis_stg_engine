using Godot;

public partial class EntitySpawnResource : EntityResource {
    public enum SpawnTrigger {
        OnCollide,
        Interval
    }

    [Export]
    public SpawnTrigger trigger;
    [Export]
    public SpawnResource otherSpawn;
    [Export]
    public float interval = 1f;
}