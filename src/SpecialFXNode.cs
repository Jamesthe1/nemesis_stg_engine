using System.Collections.Generic;
using Godot;

public partial class SpecialFXNode : Spawnable {
    [Export]
    public SpecialFXResource fxResource;

    public override SpawnResource Data { get => fxResource; set => fxResource = value as SpecialFXResource; }

    public override IEnumerable<Node2D> ConstructChildren () {
        GpuParticles2D particle = new GpuParticles2D {
            Name = "Particle",
            Amount = fxResource.count,
            Material = fxResource.material,
            Texture = fxResource.texture
        };

        particle.Finished += Despawn;
        yield return particle;

        foreach (Node2D sound in ConstructSounds ())
            yield return sound;
    }

    protected void Despawn () {
        STGController.Instance.Despawn (this);
    }
}