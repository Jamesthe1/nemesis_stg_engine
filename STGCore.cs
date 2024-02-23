#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public sealed partial class STGCore : EditorPlugin {
	private List<string> scripts = new List<string> ();

	public override void _EnterTree () {
		RegisterTypes (STGScripts.types.Keys, STGScripts.types.Values);
		RegisterResources ();
	}

	private void RegisterResources () {
		RegisterTypes (STGScripts.resources, Enumerable.Repeat (typeof (Resource), STGScripts.resources.Length));
	}

	private void RegisterTypes (IEnumerable<Type> types, IEnumerable<Type> baseTypes) {
		for (int i = 0; i < types.Count (); i++) {
			Type type = types.ElementAt (i);
			Type baseType = baseTypes.ElementAt (i);
			// Base class must be a node predefined in Godot, not a custom one
			AddCustomType (type.Name, baseType.Name, STGScripts.scripts[type.Name], null);
			scripts.Add (type.Name);
		}
	}

	public override void _ExitTree () {
		scripts.ForEach (RemoveCustomType);
		scripts.Clear ();
	}
}
#endif
