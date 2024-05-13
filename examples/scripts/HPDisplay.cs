using System.Linq;
using Godot;

public partial class HPDisplay : Label {
    private int hp;

    private void UpdateText () {
        Text = $"HP: {hp}";
    }

    private void HealthUpdate (int amount) {
        hp += amount;
        UpdateText ();
    }

    public override void _Ready () {
        STGController.Instance.StageStart += StageReadyHook;
    }

    public void StageReadyHook () {
        STGController.Instance.StageStart -= StageReadyHook;
        PlayerEntity player = STGController.Players.First ();
        hp = player.currentHp;
        UpdateText ();
        player.HealthChanged += HealthUpdate;
    }
}