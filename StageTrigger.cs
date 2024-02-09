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

    [Signal]
    public delegate void TriggerEventHandler (TriggerType type);
}