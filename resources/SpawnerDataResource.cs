using Godot;

using SpawnDictionary = Godot.Collections.Dictionary<
                            Godot.Vector2,
                            Godot.Collections.Dictionary<SpawnResource, float>
                        >;

public partial class SpawnerDataResource : SpawnResource {
    public enum SpawnTrigger {
        OnSeen,
        OnAppear,
        Event
    }

    [Export]
    public SpawnTrigger trigger = SpawnTrigger.OnSeen;
    [Export]
    public SpawnDictionary spawnPoints = new SpawnDictionary ();
    [Export]
    public bool despawnAfter = true;
    [Export]
    public bool requireKill = false;
}