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

    public NodePath spawnerPath = "";

    public bool SpawnedByPlayer { get; private set; } = false;

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
                STGController.Instance.Spawn (Data.intervalSpawn, Position, GetPath ());
        }
    }

    public override void _PhysicsProcess (double delta) {
        if (!Active)
            return;
        
        ProcessInterval (delta);
        timeElapsed += delta;
    }

    public override void _Process (double delta) {
        if (!Active)
            return;

        if (HasNode ("Sprite") && Data.fixTexRotation)
            GetNode<Node2D> ("Sprite").Rotation = -Rotation;
    }

    [Signal]
    public delegate void SpawnEventHandler ();
    [Signal]
    public delegate void DespawnEventHandler ();

    public virtual void _OnSpawn () {
        if (Data == null)
            throw new NullReferenceException ($"Spawnable data of node {Name} cannot be null");
        timeElapsed = 0.0;
        Node spawner = GetNode (spawnerPath);
        if (spawner is PlayerEntity
            || (spawner is Spawnable spawnable && spawnable.SpawnedByPlayer))
            SpawnedByPlayer = true;
    }

    public virtual void _OnDespawn () {
        if (Data.despawnSpawn != null)
            STGController.Instance.Spawn (Data.despawnSpawn, Position, GetPath ());
    }

    public virtual void _OnSeen () { }
    // Doesn't activate if we spawn outside the scene, we should be good
    public virtual void _OnUnseen () {
        STGController.Instance.Despawn (this);
    }
}