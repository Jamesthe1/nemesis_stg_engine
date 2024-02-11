using System;
using Godot;

public abstract partial class Spawnable : CharacterBody2D {
    protected double timeElapsed = 0.0;

    public abstract SpawnResource Data {
        get; set;
    }

    private bool _active = true;
    public bool Active {
        get => _active;
        set {
            _active = value;
            CollisionLayer = Data.collisionLayer * Convert.ToUInt32 (_active);
            CollisionMask = Data.collisionMask * Convert.ToUInt32 (_active);
            Visible = _active;
        }
    }

    public override void _EnterTree () {
        Spawn += _OnSpawn;
        Despawn += _OnDespawn;
    }

    public override void _ExitTree () {
        Spawn -= _OnSpawn;
        Despawn -= _OnDespawn;
    }

    protected virtual void ProcessInterval (double delta) {
        if (Data.intervalSpawn != null) {
            double te_interval = (timeElapsed % Data.interval) + delta;
            if (te_interval > Data.interval)
                STGController.Instance.Spawn (Data.intervalSpawn, Position);
        }
    }

    public override void _PhysicsProcess (double delta) {
        ProcessInterval (delta);
        timeElapsed += delta;
    }

    [Signal]
    public delegate void SpawnEventHandler ();
    [Signal]
    public delegate void DespawnEventHandler ();

    public virtual void _OnSpawn () {
        timeElapsed = 0.0;
    }

    public virtual void _OnDespawn () {
        if (Data.despawnSpawn != null)
            STGController.Instance.Spawn (Data.despawnSpawn, Position);
    }

    public virtual void _OnSeen () { }
    // Doesn't activate if we spawn outside the scene, we should be good
    public virtual void _OnUnseen () {
        EmitSignal ("Despawn");
    }
}