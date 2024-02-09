using System.Linq;
using Godot;
using Godot.Collections;

public partial class PlayerEntity : Entity {
    [Export]
    public Dictionary<string, string> inputNames = new Dictionary<string, string> () {
        { "ui_up", "up" },
        { "ui_down", "down" },
        { "ui_left", "left" },
        { "ui_right", "right" },
        { "ui_select", "fire" }
    };

    protected Dictionary<string, float> inputs = new Dictionary<string, float> ();

    public int DeviceID {
        get => ((PlayerResource)entityData).deviceID;
    }
    public bool UsesKeyboard {
        get => ((PlayerResource)entityData).usesKeyboard;
    }

    private double timeSinceFire = 0f;

    public override void _Ready () {
        foreach (string name in inputNames.Values)
            inputs.Add (name, 0f);
    }

    protected override void ProcessInterval (double delta) {
        timeSinceFire += delta;
        if (entityData.intervalSpawn != null &&
          inputs["fire"] > 0f &&
          timeSinceFire > entityData.interval) {
            STGController.Instance.Spawn (entityData.intervalSpawn, Position);
            timeSinceFire %= entityData.interval;   // We might have some delta left over, keep it
        }
    }

    protected override (float, Vector2) GetRotationAndMovement (double delta) {
        Vector2 motionAxes = new Vector2 (inputs["right"] - inputs["left"], inputs["down"] - inputs["up"]);
        return (0f, motionAxes.Normalized () * entityData.speed * (float)delta);
    }

    public override void _Input (InputEvent @event) {
        if (@event.Device != DeviceID || !Active)
            return;

        bool isJoy = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;
        if ((UsesKeyboard && isJoy) || (!UsesKeyboard && @event is InputEventKey))
            return;

        foreach (var inputName in inputNames) {
            if (@event.IsAction (inputName.Key))
                inputs[inputName.Value] = @event.GetActionStrength (inputName.Key);
        }
    }

    public override void _OnSpawn () {
        base._OnSpawn ();
        if (!(entityData is PlayerResource))
            throw new System.Exception ("A player has been spawned without a player resource. Please make sure your player spawner's data has a PlayerResource associated with it!");
    }

    public override void _OnDespawn () {
        base._OnDespawn ();
        foreach (string name in inputs.Keys)
            inputs[name] = 0f;
    }
}