using Godot;

public partial class SpawnableSounds : Resource {
    [Export]
    public AudioStream spawn;
    [Export]
    public AudioStream idle;
    [Export]
    public AudioStream destroy;
    [Export]
    public AudioStream despawn;
}