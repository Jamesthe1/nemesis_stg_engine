using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Entity : Spawnable {
    [Export]
    public EntityResource entityData;

    public override SpawnResource Data { get => entityData; set => entityData = value as EntityResource; }

    public int currentHp;
    protected int currentPhaseWeapon;

    protected WeaponStateData[] weaponStatesPhase = new WeaponStateData[0];
    protected WeaponStateData weaponState = new WeaponStateData ();
    protected double timeSinceSelected = 0.0;
    protected bool firedLastFrame = false;

    protected Vector2 lastSpawnerPos;
    protected double lastPathElapsed;

    [Export]
    public WeaponResource intervalOverride;

    protected bool IntervalOverridden () {
        return intervalOverride != null;
    }

    protected SpawnResource GetIntervalSpawn () {
        if (!IntervalOverridden ())
            return entityData.intervalSpawn;
        return intervalOverride.projectile;
    }

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

        if (IsPhased ())
            ResetPhaseStates ();
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
        bool noDir = false;
        switch (entityData.type) {
            case EntityResource.MotionType.Path: {
                Curve2D path = entityData.path;
                int endpoint = path.PointCount - 1;
                if (timeElapsed > endpoint && !entityData.loopPath) {
                    angle = path.GetPointIn (endpoint).OtherAngleTo (Vector2.Zero);
                    angle = Mathf.RadToDeg (angle) - RotationDegrees;
                    goto case EntityResource.MotionType.Standard;
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
                if (dirAngle > Mathf.Pi)
                    dirAngle = dirAngle - (Mathf.Pi * 2);   // Adds a sign for our alternative rotation
                //angle = dirAngle;
                angle = Mathf.Min (Mathf.Abs (entityData.turnSpeed) * (float)delta, Mathf.Abs (dirAngle));
                angle *= Mathf.Sign (dirAngle);
                break;
            }
            case EntityResource.MotionType.Standard: {
                noDir = STGController.Instance.movables.Contains (this);
                break;
            }
        }
        angle = Mathf.DegToRad (angle); // Everything must use radians, we've been working with degrees

        Vector2 dir = Vector2.FromAngle (Rotation + angle) * entityData.speed * (float)delta;
        return (angle, noDir ? Vector2.Zero : dir);
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

        if (timeElapsed > entityData.moveWithStageAfter && entityData.moveWithStageAfter > 0f) {
            if (!STGController.Instance.movables.Contains (this))
                STGController.Instance.movables.Add (this);
        }

        (float angle, Vector2 dir) = GetRotationAndMovement (delta);
        if (entityData.moveWithSpawner)
            dir += GetSpawnerPos () - lastSpawnerPos;

        Rotate (angle);
        var collision = MoveAndCollide (dir);
        if (collision != null) 
            ProcessCollision (collision.GetCollider ());

        lastSpawnerPos = GetSpawnerPos ();
    }

    protected virtual bool GetFiringState () {
        return true;
    }

    protected virtual void ProcessFiring (WeaponResource optionOverride, double delta, ref WeaponStateData weaponState) {
        bool overridden = optionOverride != null;

        SpawnResource spawn = overridden ? optionOverride.projectile : Data.intervalSpawn;
        float interval = overridden ? optionOverride.interval : Data.interval;
        bool autofire = !overridden || optionOverride.autofire;         // Autofire by default
        bool fireOnce = overridden ? optionOverride.fireOnce : false;

        bool fireAgain = autofire || !firedLastFrame;
        bool repeat = !fireOnce || !weaponState.fired;
        if (spawn != null &&
          fireAgain &&
          repeat &&
          weaponState.timeSinceFire > interval &&
          GetFiringState ()) {
            Spawnable projectile = STGController.Instance.Spawn (spawn, Position, GetPath ());
            if (overridden)
                projectile.RotationDegrees += optionOverride.rotationOffset;
            
            weaponState.timeSinceFire = 0;
            weaponState.fired = true;
        }
        else
            weaponState.timeSinceFire += delta; // Keep delta update in else statement, lest we have issues with how firing is processed
        firedLastFrame = GetFiringState ();
    }

    protected override void ProcessInterval (double delta) {
        if (entityData is EntityPhasedResource phasesRsc && phasesRsc.phases.Length > 0)
            ProcessPhases (delta);
        
        ProcessFiring (intervalOverride, delta, ref weaponState);
    }

    protected virtual void ProcessPhases (double delta) {
        EntityPhase phase = GetPhase ();
        WeaponStateData weaponPhaseData = weaponStatesPhase[currentPhaseWeapon];

        WeaponResource option = phase.options[currentPhaseWeapon];
        bool canFire = !option.fireOnce || !weaponPhaseData.fired;  // Fire many by default
        bool skip = !canFire && Mathf.IsZeroApprox (timeSinceSelected);
        ProcessFiring (option, delta, ref weaponPhaseData);

        weaponStatesPhase[currentPhaseWeapon] = weaponPhaseData;

        if (timeSinceSelected > option.timeUntilSwitch || skip) {
            timeSinceSelected = 0;
            firedLastFrame = false;
            weaponStatesPhase[currentPhaseWeapon].timeSinceFire = 0;

            currentPhaseWeapon++;
            currentPhaseWeapon %= phase.options.Length;
        }
        else
            timeSinceSelected += delta;
    }

    public bool IsPhased () {
        return entityData is EntityPhasedResource;
    }

    protected virtual EntityPhase GetPhase () {
        if (!IsPhased ())
            return null;
        return (entityData as EntityPhasedResource).phases
                .GetBest ((ph1, ph2) => currentHp <= ph1.hpMark && ph1.hpMark < ph2.hpMark  // We want the smallest phase that satisfies our HP
                                        || currentHp > ph2.hpMark);     // Just in case the first selected item is not what we want
    }

    protected virtual void ResetPhaseStates () {
        currentPhaseWeapon = 0;
        firedLastFrame = false;
        weaponStatesPhase = new WeaponStateData[GetPhase ().options.Length];
    }

    public virtual void Damage (int damage, Entity source) {
        if (damage == 0)
            return;
            
        EntityPhase currentPhase = GetPhase ();

        currentHp -= damage;
        EmitSignal ("HealthChanged", -damage);

        EntityPhase newPhase = GetPhase ();
        // IsPhased() is already implied since, if the data isn't phased, these would both be null
        if (currentPhase != newPhase) {
            ResetPhaseStates ();
            EmitSignal ("PhaseChanged", newPhase);
        }
        if (currentHp <= 0)
            Destroy (source != null && source.SpawnedByPlayer);
    }

    public virtual void Heal (int amount) {
        currentHp = Mathf.Min (currentHp + amount, entityData.hp);
        EmitSignal ("HealthChanged", amount);
    }

    public virtual void Destroy (bool destroyedByPlayer) {
        if (destroyedByPlayer)
            STGController.Score += entityData.score;
        if (entityData.destroySpawn != null)
            STGController.Instance.Spawn (entityData.destroySpawn, Position, GetPath ());
        if (entityData.sounds != null)
            SetCurrentSound (entityData.sounds.destroy);

        // TODO: Implement SpriteFrames animation, wait for destroy animation to complete or fire immediately if not exist
        EmitSignal ("Destroyed", destroyedByPlayer);
        STGController.Instance.Despawn (this);
    }

    public override void _OnDespawn () {
        base._OnDespawn ();
        if (STGController.Instance.movables.Contains (this))
            STGController.Instance.movables.Remove (this);
    }

    [Signal]
    public delegate void HealthChangedEventHandler (int amount);
    [Signal]
    public delegate void PhaseChangedEventHandler (EntityPhase phase);
    [Signal]
    public delegate void DestroyedEventHandler (bool destroyedByPlayer);
}