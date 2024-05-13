using System;
using System.Collections.Generic;
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
    
    public override void _Ready () {
        if (!STGController.Instance.IsTracked (this)) {
            STGController.Instance.RequestTrack (this);
            foreach (Node2D child in ConstructChildren ())
                AddChild (child);
            EmitSignal ("Spawned");
        }
    }

    public override void _EnterTree () {
        Spawned += _OnSpawn;
        FinishedSetup += _OnFinishedSetup;
        Despawned += _OnDespawn;
    }

    public override void _ExitTree () {
        Spawned -= _OnSpawn;
        FinishedSetup -= _OnFinishedSetup;
        Despawned -= _OnDespawn;
    }

    protected IEnumerable<Node2D> ConstructSprites () {
        if (Data.sequence == null)
            yield break;
        
        yield return new AnimatedSprite2D {
            Name = "Sprite",
            SpriteFrames = Data.sequence,
            Animation = Data.GetDefaultAnimation ()
        };
    }

    protected IEnumerable<Node2D> ConstructSounds () {
        if (Data.sounds == null)
            yield break;

        yield return new AudioStreamPlayer2D {
            Name = "Sound"
        };
    }

    public virtual IEnumerable<Node2D> ConstructChildren () {
        foreach (Node2D sprite in ConstructSprites ())
            yield return sprite;

        foreach (Node2D sound in ConstructSounds ())
            yield return sound;

        // TODO: Start with collision deactivated
        if (Data.collisionShape != null)
            yield return new CollisionShape2D {
                Name = "Collision",
                Shape = Data.collisionShape
            };

        Vector2 size = Vector2.One * 20;
        if (Data.sequence != null)
            size = Data.GetAnimationSize ();
            
        var visCheck = new VisibleOnScreenNotifier2D {
            Name = "VisCheck",
            Rect = size.GetCenteredRegion ()
        };
        visCheck.ScreenEntered += _OnSeen;
        visCheck.ScreenExited += _OnUnseen;
        yield return visCheck;
    }

    protected void SetCurrentSound (AudioStream stream) {
        AudioStreamPlayer2D sound = GetNode<AudioStreamPlayer2D> ("Sound");
        if (sound.Playing)
            sound.Stop ();
        sound.Stream = stream;
        if (stream != null)
            sound.Play ();
    }

    protected Vector2 GetSpawnerPos () {
        return GetNode<Node2D> (spawnerPath).Position;
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
    public delegate void SpawnedEventHandler ();
    [Signal]
    public delegate void FinishedSetupEventHandler ();
    [Signal]
    public delegate void DespawnedEventHandler ();

    public virtual void _OnSpawn () {
        void SpawnAnimHook () {
            GetNode<AnimatedSprite2D> ("Sprite").AnimationFinished -= SpawnAnimHook;
            EmitSignal ("FinishedSetup");
        }

        // Keep separate from anim hook to let the spawn sound complete
        void SpawnSoundHook () {
            GetNode<AudioStreamPlayer2D> ("Sound").Finished -= SpawnSoundHook;
            SetCurrentSound (Data.sounds.idle);
        }

        if (Data == null)
            throw new NullReferenceException ($"Spawnable data of node {Name} cannot be null");

        timeElapsed = 0.0;
        if (Data.sounds != null) {
            if (Data.sounds.spawn != null) {
                SetCurrentSound (Data.sounds.spawn);
                GetNode<AudioStreamPlayer2D> ("Sound").Finished += SpawnSoundHook;
            }
            else
                SetCurrentSound (Data.sounds.idle);
        }
        
        if (Data.HasAnimation ("spawn")) {
            AnimatedSprite2D sprite = GetNode<AnimatedSprite2D> ("Sprite");
            sprite.AnimationFinished += SpawnAnimHook;
            sprite.Play ("spawn");
        } else
            EmitSignal ("FinishedSetup");

        if (spawnerPath == "")
            return;
        Node spawner = GetNode (spawnerPath);
        if (spawner is PlayerEntity
            || (spawner is Spawnable spawnable && spawnable.SpawnedByPlayer))
            SpawnedByPlayer = true;
    }

    public virtual void _OnFinishedSetup () {
        if (!Data.IsAssignedTextures ())
            return;
        
        AnimatedSprite2D sprite = GetNode<AnimatedSprite2D> ("Sprite");
        sprite.Play (Data.GetDefaultAnimation ());  // Automatically chooses idle animation
    }

    public virtual void _OnDespawn () {
        if (Data.despawnSpawn != null)
            STGController.Instance.Spawn (Data.despawnSpawn, Position, GetPath ());
        if (Data.sounds != null)
            SetCurrentSound (Data.sounds.despawn);
    }

    public virtual void _OnSeen () { }
    // Doesn't activate if we spawn outside the scene, we should be good
    public virtual void _OnUnseen () {
        STGController.Instance.Despawn (this);
    }
}