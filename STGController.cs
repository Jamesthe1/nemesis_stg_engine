using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class STGController : Node2D {
    private List<Spawnable> active = new List<Spawnable> ();
    private List<Spawnable> spare = new List<Spawnable> ();

    public static List<PlayerEntity> Players { get; private set; }
    public static Dictionary<int, PlayerEntity> Devices {
        get {
            int[] devices = Input.GetConnectedJoypads ().ToArray ();
            return devices.ToDictionary (d => d, d => Players.FirstOrDefault (p => p.DeviceID == d));
        }
    }

    [Export]
    public Vector2 stageMovement = Vector2.Zero;

    public static STGController Instance {
        get; private set;
    }

    public override void _EnterTree () {
        Instance = this;
        Players = new List<PlayerEntity> ();
    }

    public void MoveStage (Vector2 direction) {
        Position += -direction;
        foreach (PlayerEntity player in Players)
            player.Position += direction;   // Keep them on-screen
    }

    public override void _PhysicsProcess (double delta) {
        if (Engine.IsEditorHint ())
            return;

        MoveStage (stageMovement);
    }

    private Rect2 CenteredRegion (Vector2 size) {
        return new Rect2 (-size * 0.5f, size);
    }

    /// <summary>
    /// Spawns a spawnable using the provided resource details
    /// </summary>
    /// <returns>The new object that was spawned</returns>
    public Spawnable Spawn (SpawnResource resource, Vector2 pos) {
        bool hadToSpawn = false;
        Spawnable spawnable = spare.FirstOrDefault (s => s.Data.baseScene.Equals (resource.baseScene));
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

        spawnable.SetChildIfExist ("Sprite", "texture", resource.texture);
        spawnable.SetChildIfExist ("Collision", "shape", resource.collisionShape);
        if (spawnable.HasNode ("VisCheck")) {
            Vector2 size = Vector2.One * 20;
            if (resource.texture != null)
                size = resource.texture.GetSize ();
            spawnable.SetChildIfExist ("VisCheck", "rect", CenteredRegion (size));
        }

        spawnable.Data = resource;
        spawnable.Active = true;    // Also sets collision mask and layer
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
    /// <returns>A state whether or not the object could be despawned</returns>
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