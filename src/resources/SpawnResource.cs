using System.Linq;
using Godot;

public abstract partial class SpawnResource : Resource {
    [Export]
    public string name = "Node";
    [Export]
    public SpriteFrames sequence;
    [Export]
    public bool fixTexRotation = false;
    [Export]
    public Script scriptOverride = null;
    [Export]
    public SpawnableSounds sounds;

    [Export]
    public Shape2D collisionShape;
    [Export]
    public uint collisionLayer = 1;
    [Export]
    public uint collisionMask = 1;

    /// <summary>
    /// Spawned on despawn
    /// </summary>
    [Export]
    public SpawnResource despawnSpawn;
    [Export]
    public SpawnResource intervalSpawn;
    /// <summary>
    /// The interval to spawn <see cref="intervalSpawn"/>
    /// </summary>
    [Export]
    public float interval = 1f;

    public abstract Script GetDefaultScript ();

    public Script GetRelatedScript () {
        return scriptOverride ?? GetDefaultScript ();
    }

    public bool HasAnimation (string animationName) {
        return sequence.GetAnimationNames ().Contains (animationName);
    }

    public string GetDefaultAnimation () {
        if (HasAnimation ("idle"))
            return "idle";
        if (HasAnimation ("default"))
            return "default";
        return sequence.GetAnimationNames ()[0];
    }

    public Vector2 GetAnimationSize () {
        return sequence.GetFrameTexture (GetDefaultAnimation (), 0).GetSize ();
    }
}