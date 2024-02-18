using Godot;

public partial class WeaponResource : Resource {
    [Export]
    public SpawnResource projectile;
    [Export]
    public bool autofire = true;
    [Export]
    public float interval = 1f;
}