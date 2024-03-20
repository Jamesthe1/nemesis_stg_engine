using System.Collections.Generic;
using Godot;

public partial class StageTrigger : Marker2D, ISaveState<bool> {
    public enum TriggerCondition {
        PassX,
        PassY,
        OnSeen
    }

    public enum TriggerType {
        EventOnly,
        Checkpoint,
        JumpToNode,
        ChangeStageMovement,
        Boss
    }

    [Export]
    public TriggerCondition condition;
    [Export]
    public TriggerType type;
    [Export]
    public NodePath jump;
    [Export]
    public Vector2 stageMovement;

    [Export]
    public bool fireOnce = true;

    public bool disabled = false;

    public static Dictionary<NodePath, bool> States { get; private set; } = new Dictionary<NodePath, bool> ();

    public override void _Ready () {
        string visCheckName = "VisCheck";
        if (!HasNode (visCheckName)) {
            var visCheck = this.CreateChild<VisibleOnScreenNotifier2D> (visCheckName);
            visCheck.ScreenEntered += ScreenTriggerCheck;
        }
    }

    protected bool Passed (int axis) {
        Vector2 stage = STGController.Instance.StagePos;
        Vector2 moveDir = STGController.Instance.stageMovement.Sign ();
        if (moveDir[axis] == 1)
            return stage[axis] > Position[axis];
        return stage[axis] < Position[axis];
    }

    public override void _EnterTree () {
        STGController.Instance.SaveCheckpoint += SaveState;
        STGController.Instance.LoadCheckpoint += LoadState;
    }

    public override void _ExitTree () {
        GetNode<VisibleOnScreenNotifier2D> ("VisCheck").ScreenEntered -= ScreenTriggerCheck;
        STGController.Instance.SaveCheckpoint -= SaveState;
        STGController.Instance.LoadCheckpoint -= LoadState;
    }

    public override void _PhysicsProcess (double delta) {
        if (disabled)
            return;

        if (condition == TriggerCondition.PassX && Passed (0) ||
            condition == TriggerCondition.PassY && Passed (1))
            FireTrigger ();
    }

    public void ScreenTriggerCheck () {
        if (condition == TriggerCondition.OnSeen)
            FireTrigger ();
    }

    protected virtual void FireTrigger () {
        EmitSignal ("Triggered");
        switch (type) {
            case TriggerType.Checkpoint: {
                STGController.SetNewCheckpoint (GetPath ());
                break;
            }
            case TriggerType.JumpToNode: {
                if (jump != null && jump != "")
                    STGController.Instance.MoveStageTo (jump, this);
                break;
            }
            case TriggerType.ChangeStageMovement: {
                STGController.Instance.stageMovement = stageMovement;
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

    public void SaveState () {
        States[GetPath ()] = disabled;
    }

    public void LoadState () {
        disabled = States[GetPath ()];
    }

    [Signal]
    public delegate void TriggeredEventHandler ();
}