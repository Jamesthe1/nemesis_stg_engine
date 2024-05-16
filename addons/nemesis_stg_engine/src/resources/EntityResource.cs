using Godot;

/// <summary>
/// An entity with preset movement behavior.
/// <para><see cref="MotionType.Standard"/>: The entity will move in a straight line (or turn consistently by <see cref="turnSpeed"/>) until off-screen.</para>
/// <para><see cref="MotionType.Path"/>: The entity will follow <see cref="path"/>. Once complete, it will follow default behavior unless <see cref="loopPath"/> is true.</para>
/// <para><see cref="MotionType.Follow"/>: The entity will turn towards <see cref="follow"/> by <see cref="turnSpeed"/>.</para>
/// </summary>
public partial class EntityResource : SpawnResource {
    public enum MotionType {
        Standard,
        Path,
        Follow
    }
    [Export]
    public MotionType type;
    [Export]
    public Curve2D path;
    [Export]
    public bool loopPath = false;
    [Export]
    public NodePath follow;

    [Export]
    public float speed = 1f;
    [Export]
    public float turnSpeed = 0f;
    [Export]
    public bool moveWithSpawner = false;
    [Export]
    public float moveWithStageAfter = 0f;

    [Export]
    public int ramDamage = 1;
    /// <summary>
    /// Damage for when there's collision with something that is not an entity
    /// </summary>
    [Export]
    public int miscSelfDamage = 1;
    /// <summary>
    /// Health of the moving item. If set to 0, despawns on collide
    /// </summary>
    [Export]
    public int hp = 3;
    [Export]
    public int score = 250;
    [Export]
    public bool isBoss = false;
    [Export]
    public bool endsStage = false;

    [Export]
    public SpawnResource destroySpawn;

    public override Script GetDefaultScript () {
        return STGScripts.scripts[nameof (Entity)];
    }
}