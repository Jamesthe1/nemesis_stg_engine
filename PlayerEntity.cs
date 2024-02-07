using Godot;
using Godot.Collections;

public partial class PlayerEntity : Entity {
    [Export]
    public Dictionary<string, string> inputNames = new Dictionary<string, string> () {
        { "ui_up", "up" },
        { "ui_down", "down" },
        { "ui_left", "left" },
        { "ui_right", "right" }
    };

    [Export]
    public int deviceID;

    [Export]
    public bool controller;

    protected Dictionary<string, float> inputs = new Dictionary<string, float> ();

    public override void _Ready () {
        foreach (string name in inputNames.Values)
            inputs.Add (name, 0f);
    }

    protected override (float, Vector2) GetRotationAndMovement (double delta) {
        Vector2 motionAxes = new Vector2 (inputs["right"] - inputs["left"], inputs["down"] - inputs["up"]);
        return (0f, motionAxes.Normalized () * entityData.speed * (float)delta);
    }

    public override void _Input (InputEvent @event) {
        if (@event.Device != deviceID)
            return;

        if (@event is InputEventAction e_action && inputNames.ContainsKey (e_action.Action)) {
            string key = inputNames[e_action.Action];
            inputs[key] = e_action.Strength;
        }
    }
}