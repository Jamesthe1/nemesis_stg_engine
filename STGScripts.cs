using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class STGScripts {
    public const string root = "res://addons/nemesis_stg_engine/src";

    public static Dictionary<Type, Type> types = new Dictionary<Type, Type> {
        { typeof(STGController), typeof(Node2D) },
		{ typeof(Spawner), typeof(CharacterBody2D) },
		{ typeof(Entity), typeof(CharacterBody2D) },
		{ typeof(PlayerEntity), typeof(CharacterBody2D) },
		{ typeof(SpecialFXNode), typeof(CharacterBody2D) },
		{ typeof(Pickup), typeof(CharacterBody2D) },
		{ typeof(StageTrigger), typeof(Marker2D) },
    };

    public static Type[] resources = new Type[] {
        typeof(EntityResource),
		typeof(SpawnerDataResource),
		typeof(PlayerResource),
		typeof(PickupResource),
		typeof(WeaponResource),
		typeof(EntityPhasedResource),
		typeof(EntityPhase),
		typeof(SpecialFXResource),
    };

    private static Dictionary<string, Script> TransformToScripts (this IEnumerable<Type> types, bool isResource = false) {
        string folder = root;
        if (isResource)
            folder += "/resources";
        return types.Select (t => (t.Name, GD.Load<Script> ($"{folder}/{t.Name}.cs")))
                    .ToDictionary (kv => kv.Name, kv => kv.Item2);
    }

    private static Dictionary<string, Script> GenerateScripts () {
        return types.Keys.TransformToScripts ()
                    .Concat (resources.TransformToScripts (true))
                    .ToDictionary (kv => kv.Key, kv => kv.Value);
    }

    // Save as cache to prevent continual re-generation
    public static Dictionary<string, Script> scripts = GenerateScripts ();
}