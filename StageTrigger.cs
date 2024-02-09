using Godot;

public partial class StageTrigger : Marker2D {
    public enum TriggerCondition {
        PassX,
        PassY,
        OnSeen
    }

    public enum TriggerType {
        Checkpoint,
        JumpToTrigger,
        Boss
    }

    [Export]
    public TriggerCondition condition;
    [Export]
    public TriggerType type;
    [Export]
    public NodePath jump;

    [Export]
    public bool fireOnce = true;

    public bool disabled;

    public override void _Ready () {
        string visCheckName = "VisCheck";
        if (!HasNode (visCheckName)) {
            var visCheck = this.CreateChild<VisibleOnScreenNotifier2D> (visCheckName);
            visCheck.ScreenEntered += ScreenTriggerCheck;
        }
    }

    public override void _ExitTree () {
        GetNode<VisibleOnScreenNotifier2D> ("VisCheck").ScreenEntered -= ScreenTriggerCheck;
    }

    public override void _PhysicsProcess (double delta) {
        bool Passed (int axis) {
            Vector2 stage = -STGController.Instance.Position;
            Vector2 moveDir = STGController.Instance.stageMovement.Sign ();
            if (moveDir[axis] == 1)
                return stage[axis] >= Position[axis];
            return stage[axis] <= Position[axis];
        }

        if (condition == TriggerCondition.PassX && Passed (0) ||
            condition == TriggerCondition.PassY && Passed (1))
            FireTrigger ();
    }

    public void ScreenTriggerCheck () {
        if (condition == TriggerCondition.OnSeen)
            FireTrigger ();
    }

    private void FireTrigger () {
        if (!disabled)
            EmitSignal ("Trigger", this);
        if (fireOnce)
            disabled = true;
    }

    [Signal]
    public delegate void TriggerEventHandler (StageTrigger triggerNode);
}