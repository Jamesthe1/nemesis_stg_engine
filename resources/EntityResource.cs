using Godot;

/// <summary>
/// An entity with preset movement behavior.
/// <para>If nether <see cref="path"/> nor <see cref="follow"/> are set, the entity will move in a straight line (or turn consistently by <see cref="turnSpeed"/>) until off-screen.</para>
/// <para>If <see cref="path"/> is set, the entity will follow the path. It will then follow default behavior unless <see cref="loopPath"/> is true.</para>
/// <para>If <see cref="follow"/> is set (and <see cref="path"/> is null), the entity will turn towards the node (if it can be found) by <see cref="turnSpeed"/>.</para>
/// </summary>
public partial class EntityResource : SpawnResource {
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
    public int damage = 1;
    /// <summary>
    /// Damage for when there's collision with something that is not an entity
    /// </summary>
    [Export]
    public int miscDamage = 1;
    /// <summary>
    /// Health of the moving item. If set to 0, despawns on collide
    /// </summary>
    [Export]
    public int hp = 3;
}