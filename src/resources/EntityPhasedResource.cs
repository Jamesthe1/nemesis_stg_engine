using Godot;

public partial class EntityPhasedResource : EntityResource {
    [Export]
    public EntityPhase[] phases = new EntityPhase[0];
}