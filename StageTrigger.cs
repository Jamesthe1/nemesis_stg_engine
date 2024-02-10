using System.Collections;
using Godot;

public partial class StageTrigger : Marker2D {
    public enum TriggerCondition {
        PassX,
        PassY,
        OnSeen
    }

    public enum TriggerType {
        Checkpoint,
        JumpToNode,
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

    protected virtual void FireTrigger () {
        if (disabled)
            return;

        switch (type) {
            case TriggerType.Checkpoint: {
                // TODO: Checkpoint code
                break;
            }
            case TriggerType.JumpToNode: {
                if (jump != null && jump != "")
                    STGController.Instance.MoveStageTo (jump);
                break;
            }
            case TriggerType.Boss: {
                STGController.Instance.EmitSignal ("BossAlarm");
                break;
            }
        }

        if (fireOnce)
            disabled = true;
    }
}