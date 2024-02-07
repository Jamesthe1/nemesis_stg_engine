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

    [Signal]
    public delegate void SpawnEventHandler ();
    [Signal]
    public delegate void DespawnEventHandler ();

    public virtual void _OnSpawn () {
        timeElapsed = 0.0;
    }
    public virtual void _OnDespawn () { }
}