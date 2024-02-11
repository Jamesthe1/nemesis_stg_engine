using System;
using Godot;

public partial class Entity : Spawnable {
    [Export]
    public EntityResource entityData;

    public override SpawnResource Data { get => entityData; set => entityData = value as EntityResource; }

    public int currentHp;

    public override void _EnterTree () {
        base._EnterTree ();
        Damage += _OnDamage;
    }

    public override void _ExitTree () {
        base._ExitTree ();
        Damage -= _OnDamage;
    }

    public override void _OnSpawn () {
        base._OnSpawn ();
        currentHp = entityData.hp;
    }

    /// <summary>
    /// Gets where the craft should rotate and move this physics frame
    /// </summary>
    /// <returns>A tuple representing rotation and movement</returns>
    protected virtual (float, Vector2) GetRotationAndMovement (double delta) {
        float angle = entityData.turnSpeed;
        switch (entityData.type) {
            case EntityResource.MotionType.Path: {
                // TODO: Get new angle from path
                break;
            }
            case EntityResource.MotionType.Follow: {
                Vector2 pos = GetNode<Node2D> (entityData.follow).Position;
                float dirAngle = GetAngleTo (pos);
                angle = (float)Math.Min (Mathf.Abs (entityData.turnSpeed) * delta, Mathf.Abs (dirAngle));
                angle *= Mathf.Sign (dirAngle);
                break;
            }
        }
        angle = Mathf.DegToRad (angle); // Everything must use radians, we've been working with degrees

        Vector2 dir = Vector2.FromAngle (Rotation + angle) * entityData.speed * (float)delta;
        return (angle, dir);
    }

    public override void _PhysicsProcess (double delta) {
        base._PhysicsProcess (delta);

        (float angle, Vector2 dir) = GetRotationAndMovement (delta);
        Rotate (angle);
        var collision = MoveAndCollide (dir);
        if (collision != null) {
            if (collision.GetCollider () is Entity other) {
                other.EmitSignal ("Damage", entityData.damage);
                EmitSignal ("Damage", other.entityData.damage);
            }
            else
                EmitSignal ("Damage", entityData.selfDamage);
        }
    }

    [Signal]
    public delegate void DamageEventHandler (int damage);

    public virtual void _OnDamage (int damage) {
        if (damage == 0)
            return;
            
        currentHp -= damage;
        if (currentHp <= 0)
            STGController.Instance.Despawn (this);
    }
}