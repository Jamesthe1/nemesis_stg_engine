using Godot;

public partial class PlayerResource : EntityResource {
    [Export]
    public int deviceID;
    [Export]
    public bool usesKeyboard;

    public override Script GetDefaultScript () {
        return STGScripts.scripts[nameof (PlayerEntity)];
    }
}