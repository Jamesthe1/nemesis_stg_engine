using System.Collections.Generic;
using Godot;

public partial class SpecialFXNode : Spawnable {
    [Export]
    public SpecialFXResource fxResource;

    public override SpawnResource Data { get => fxResource; set => fxResource = value as SpecialFXResource; }

    public override IEnumerable<Node2D> ConstructChildren () {
        foreach (Node2D child in base.ConstructChildren ())
            yield return child;

        GpuParticles2D particle = new GpuParticles2D {
            Name = "Particle",
            Amount = fxResource.count,
            ProcessMaterial = fxResource.material,
            Texture = fxResource.particle,
            OneShot = true,
            Lifetime = fxResource.emissionTime
        };

        particle.Finished += Despawn;
        yield return particle;
    }

    protected void Despawn () {
        STGController.Instance.Despawn (this);
    }
}