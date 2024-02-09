using System;
using Godot;

public abstract partial class Spawnable : CharacterBody2D {
    protected double timeElapsed = 0.0;

    public abstract SpawnResource Data {
        get; set;
    }

    public bool Active {
        get => (CollisionLayer | CollisionMask) > 0 && Visible;
        set {
            CollisionLayer = Data.collisionLayer * Convert.ToUInt32 (value);
            CollisionMask = Data.collisionMask * Convert.ToUInt32 (value);
            Visible = value;
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
    public virtual void _OnUnseen () {
        EmitSignal ("Despawn");
    }

    public void SetChildIfExist (NodePath path, string param, Variant value) {
        if (HasNode (path))
            GetNode (path).Set (param, value);
    }
}