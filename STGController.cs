using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public sealed partial class STGController : Node2D {
    private List<Spawnable> active = new List<Spawnable> ();
    private List<Spawnable> spare = new List<Spawnable> ();

    [Export]
    public Vector2 stageMovement = Vector2.Zero;

    public static STGController Instance {
        get; private set;
    }

    public override void _EnterTree () {
        Instance = this;
    }

    public override void _PhysicsProcess (double delta) {
        Position += -stageMovement;
    }

    /// <summary>
    /// Spawns a spawnable using the provided resource details
    /// </summary>
    /// <returns>The new object that was spawned</returns>
    public Spawnable Spawn (SpawnResource resource, Vector2 pos) {
        bool hadToSpawn = false;
        Spawnable spawnable = spare.FirstOrDefault (s => s.Data.baseScene == resource.baseScene);
        if (spawnable == null) {
            hadToSpawn = true;
            spawnable = resource.baseScene.Instantiate () as Spawnable;
        }
        else
            spare.Remove (spawnable);
        active.Add (spawnable);

        // Moving this after adding to active due to _EnterTree event
        if (hadToSpawn)
            AddChild (spawnable);

        resource.SetStatsOf (spawnable);
        spawnable.Active = true;
        spawnable.Position = pos;
        spawnable.EmitSignal ("Spawn");
        return spawnable;
    }

    /// <summary>
    /// Checks whether or not the spawnable is being handled by the STG controller
    /// </summary>
    /// <returns>A state whether or not this node's spawn behavior is being handled by the STG controller</returns>
    public bool IsTracked (Spawnable spawnable) {
        return active.Contains (spawnable) || spare.Contains (spawnable);
    }

    /// <summary>
    /// Requests to manage the handling of a spawnable
    /// </summary>
    /// <returns>A state whether or not the request could be fulfilled</returns>
    public bool RequestTrack (Spawnable spawnable) {
        if (IsTracked (spawnable))
            return false;
        if (spawnable.Active)
            active.Add (spawnable);
        else
            spare.Add (spawnable);
        return true;
    }

    /// <summary>
    /// Despawns a spawnable
    /// </summary>
    /// <returns>A state whether or not the object could not be despawned</returns>
    public bool Despawn (Spawnable spawnable) {
        if (!active.Contains (spawnable))
            return false;
        active.Remove (spawnable);
        spare.Add (spawnable);

        spawnable.Active = false;
        spawnable.EmitSignal ("Despawn");
        return true;
    }
}