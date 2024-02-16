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
    // TODO: Implement weapon/option resource
}