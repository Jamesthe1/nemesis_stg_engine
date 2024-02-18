using Godot;

public partial class Pickup : Spawnable {
    [Export]
    public PickupResource pickupData;

    public override SpawnResource Data { get => pickupData; set => pickupData = value as PickupResource; }

    public void DoPickUp (PlayerEntity player) {
        switch (pickupData.type) {
            case PickupResource.PickupType.Health: {
                player.Heal (pickupData.value);
                break;
            }
            case PickupResource.PickupType.ScoreBonus: {
                STGController.Score += pickupData.value;
                break;
            }
            case PickupResource.PickupType.WeaponOption: {
                player.intervalOverride = pickupData.weapon;
                break;
            }
        }
        EmitSignal ("PickedUp");
        STGController.Instance.Despawn (this);
    }

    [Signal]
    public delegate void PickedUpEventHandler ();
}