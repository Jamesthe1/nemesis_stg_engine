using Godot;

public partial class SpecialFXResource : SpawnResource {
    [Export]
    public ParticleProcessMaterial material;
    [Export]
    public int count;
    [Export]
    public float emissionTime = 1f;

    public override Script GetDefaultScript () {
        return STGScripts.scripts[nameof (SpecialFXNode)];
    }
}