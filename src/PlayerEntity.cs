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

    protected double timeSinceFire = 0f;
    protected bool fired = false;
    
    protected Rect2 spriteRegion;

    public override void _Ready () {
        foreach (string name in inputNames.Values)
            inputs.Add (name, 0f);
    }

    protected override void ProcessInterval (double delta) {
        SpawnResource spawn = GetIntervalSpawn ();
        float interval = IntervalOverridden () ? intervalOverride.interval : Data.interval;

        timeSinceFire += delta;
        bool fireAgain = !IntervalOverridden () || intervalOverride.autofire || !fired; // Autofire by default
        if (spawn != null &&
          inputs["fire"] > 0f &&
          fireAgain &&
          timeSinceFire > interval) {
            STGController.Instance.Spawn (spawn, Position, GetPath ());
            timeSinceFire = 0;  // Don't keep delta since it can be abused to charge up shots
        }
        fired = inputs["fire"] > 0f;
    }

    protected override (float, Vector2) GetRotationAndMovement (double delta) {
        Vector2 motionAxes = new Vector2 (inputs["right"] - inputs["left"], inputs["down"] - inputs["up"]);
        Vector2 dir = motionAxes.Normalized () * entityData.speed * (float)delta;

        Vector2 nextPos = (Position + dir).KeepInsideScreen (spriteRegion, GetViewport ());
        return (0f, nextPos - Position);
    }

    public override void ProcessCollision (GodotObject collider) {
        base.ProcessCollision (collider);
        if (collider is Pickup pickup)
            pickup.DoPickUp (this);
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
        spriteRegion = GetNode<Sprite2D> ("Sprite").Texture.GetSize ().GetCenteredRegion ();
        if (!(entityData is PlayerResource))
            throw new System.Exception ("A player has been spawned without a player resource. Please make sure your player spawner's data has a PlayerResource associated with it!");
        STGController.Players.Add (this);
    }

    public override void _OnDespawn () {
        base._OnDespawn ();
        STGController.Players.Remove (this);
        foreach (string name in inputs.Keys)
            inputs[name] = 0f;
    }
}