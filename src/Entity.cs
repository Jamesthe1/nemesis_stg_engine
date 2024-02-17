using System;
using Godot;

public partial class Entity : Spawnable {
    [Export]
    public EntityResource entityData;

    public override SpawnResource Data { get => entityData; set => entityData = value as EntityResource; }

    public int currentHp;

    protected Vector2 lastSpawnerPos;

    protected double lastPathElapsed;

    protected Vector2 GetSpawnerPos () {
        return GetNode<Node2D> (spawnerPath).Position;
    }

    public override void _EnterTree () {
        base._EnterTree ();
    }

    public override void _ExitTree () {
        base._ExitTree ();
    }

    public override void _OnSpawn () {
        base._OnSpawn ();
        currentHp = entityData.hp;
        lastSpawnerPos = GetSpawnerPos ();
        lastPathElapsed = timeElapsed;
    }

    protected float GetRotationOnPath (float time, float previousTime) {
        Curve2D path = entityData.path;

        Vector2 point = path.Samplef (time);
        Vector2 lastPoint = path.Samplef (previousTime);
        if (time < previousTime)
            point.X += lastPoint.X;

        return lastPoint.OtherAngleTo (point);
    }

    /// <summary>
    /// Gets where the craft should rotate and move this physics frame
    /// </summary>
    /// <returns>A tuple representing rotation and movement</returns>
    protected virtual (float, Vector2) GetRotationAndMovement (double delta) {
        float angle = entityData.turnSpeed;
        switch (entityData.type) {
            case EntityResource.MotionType.Path: {
                Curve2D path = entityData.path;
                int endpoint = path.PointCount - 1;
                if (timeElapsed > endpoint && !entityData.loopPath) {
                    angle = path.GetPointIn (endpoint).OtherAngleTo (Vector2.Zero);
                    angle = Mathf.RadToDeg (angle) - RotationDegrees;
                    break;
                }

                double elapsed = timeElapsed % endpoint;
                angle = Mathf.RadToDeg (GetRotationOnPath ((float)elapsed, (float)lastPathElapsed)) - RotationDegrees;
                lastPathElapsed = elapsed;
                break;
            }
            case EntityResource.MotionType.Follow: {
                if (!HasNode (entityData.follow))
                    break;

                Vector2 pos = GetNode<Node2D> (entityData.follow).Position;
                float dirAngle = Mathf.RadToDeg (GetAngleTo (pos));
                angle = Mathf.Min (Mathf.Abs (entityData.turnSpeed) * (float)delta, Mathf.Abs (dirAngle));
                angle *= Mathf.Sign (dirAngle);
                break;
            }
        }
        angle = Mathf.DegToRad (angle); // Everything must use radians, we've been working with degrees

        Vector2 dir = Vector2.FromAngle (Rotation + angle) * entityData.speed * (float)delta;
        return (angle, dir);
    }

    public virtual void ProcessCollision (GodotObject collider) {
        if (collider is Entity other) {
            other.Damage (entityData.ramDamage, this);
            Damage (other.entityData.ramDamage, other);
        }
        else if (collider is StaticBody2D)
            Damage (entityData.miscSelfDamage, null);
    }

    public override void _PhysicsProcess (double delta) {
        base._PhysicsProcess (delta);
        if (!Active)
            return;

        (float angle, Vector2 dir) = GetRotationAndMovement (delta);
        if (entityData.moveWithSpawner)
            dir += GetSpawnerPos () - lastSpawnerPos;

        Rotate (angle);
        var collision = MoveAndCollide (dir);
        if (collision != null) 
            ProcessCollision (collision.GetCollider ());

        lastSpawnerPos = GetSpawnerPos ();
    }

    public virtual void Damage (int damage, Entity source) {
        if (damage == 0)
            return;
            
        currentHp -= damage;
        EmitSignal ("Damaged", damage, source);
        if (currentHp <= 0)
            Destroy (source != null && source.SpawnedByPlayer);
    }

    public void Heal (int amount) {
        currentHp = Mathf.Min (currentHp + amount, entityData.hp);
        EmitSignal ("Healed", amount);
    }

    public virtual void Destroy (bool destroyedByPlayer) {
        if (destroyedByPlayer)
            STGController.Score += entityData.score;
        if (entityData.destroySpawn != null)
            STGController.Instance.Spawn (entityData.destroySpawn, Position, GetPath ());

        EmitSignal ("Destroyed", destroyedByPlayer);
        STGController.Instance.Despawn (this);
    }

    [Signal]
    public delegate void HealedEventHandler (int amount);
    [Signal]
    public delegate void DamagedEventHandler (int damage, Entity source);
    [Signal]
    public delegate void DestroyedEventHandler (bool destroyedByPlayer);
}