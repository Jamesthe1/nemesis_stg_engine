using Godot;

public partial class PickupResource : SpawnResource {
    public enum PickupType {
        Health,
        WeaponOption,
        ScoreBonus
    }

    [Export]
    public PickupType type;

    [Export]
    public int value;
    [Export]
    public WeaponResource weapon;
}