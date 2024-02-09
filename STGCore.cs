#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public sealed partial class STGCore : EditorPlugin {
	private List<string> scripts = new List<string> ();

	private const string root = "res://addons/nemesis_stg_engine";

	public override void _EnterTree () {
		// Nodes
		RegisterType<Spawner, CharacterBody2D> ();
		RegisterType<Entity, CharacterBody2D> ();
		RegisterType<PlayerEntity, CharacterBody2D> ();
		RegisterType<STGController, Node2D> ();

		// Resources
		RegisterType<EntityResource, Resource> ();
		RegisterType<SpawnerDataResource, Resource> ();
		RegisterType<PlayerResource, Resource> ();
	}

	private void RegisterType<T, U> () where T : class, U where U : class {
		string folder = root;
		if (typeof (U).Name == nameof (Resource))
			folder += "/resources";
		
		Type type = typeof(T);
		Script script = GD.Load<Script> ($"{folder}/{type.Name}.cs");
		// Base class must be a node predefined in Godot, not a custom one
		AddCustomType (type.Name, typeof (U).Name, script, null);
		scripts.Add (type.Name);
	}

	public override void _ExitTree () {
		scripts.ForEach (RemoveCustomType);
		scripts.Clear ();
	}
}
#endif
