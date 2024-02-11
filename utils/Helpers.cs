using Godot;

public static class Helpers {
    public static T CreateChild<T> (this Node node, string name) where T : Node, new() {
        T child = new T ();
        node.AddChild (child);
        return child;
    }

    public static void SetChildIfExist (this Node node, NodePath path, string param, Variant value) {
        if (node.HasNode (path))
            node.GetNode (path).Set (param, value);
    }
}