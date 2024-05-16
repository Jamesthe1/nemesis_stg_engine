using Godot;

public partial class EntityPhase : Resource {
    [Export]
    public int hpMark = 3;
    [Export]
    public WeaponResource[] options = new WeaponResource[0];
}