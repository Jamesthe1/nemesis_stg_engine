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

    public static Rect2 GetCenteredRegion (this Vector2 size) {
        return new Rect2 (-size * 0.5f, size);
    }
}