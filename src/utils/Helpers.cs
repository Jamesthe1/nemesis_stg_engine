using System;
using System.Collections.Generic;
using System.Linq;
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

    public static float ClampAxis (this Vector2 vector, int axis, Rect2 surrounding, Rect2 bounds) {
        float axisValue = vector[axis];
        Vector2 minMax = new Vector2 (bounds.Position[axis] - surrounding.Position[axis], bounds.End[axis] - surrounding.End[axis]);
        return (float)Mathf.Clamp (axisValue, minMax.X, minMax.Y);
    }

    public static Vector2 ClampVector (this Vector2 vector, Rect2 surrounding, Rect2 bounds) {
        return new Vector2 (vector.ClampAxis (0, surrounding, bounds),
                            vector.ClampAxis (1, surrounding, bounds));
    }

    public static Vector2 KeepInsideScreen (this Vector2 vector, Rect2 surrounding, Viewport viewport) {
        Camera2D camera = viewport.GetCamera2D ();
        Vector2 windowSize = DisplayServer.WindowGetSize ();
        Rect2 windowRect = windowSize.GetCenteredRegion ();
        if (camera.AnchorMode == Camera2D.AnchorModeEnum.FixedTopLeft)
            windowRect = new Rect2 (Vector2.Zero, windowSize);
        windowRect.Position += camera.Position;

        return vector.ClampVector (surrounding, windowRect);
    }

    /// <summary>
    /// An implementation of <see cref="Vector2.AngleTo(Vector2)"/> that seems to work when AngleTo does not
    /// </summary>
    /// <returns>The angle to the point in space</returns>
    public static float OtherAngleTo (this Vector2 a, Vector2 b) {
        return (b - a).Angle ();
    }

    /// <summary>
    /// Compares all elements in an <see cref="IEnumerable{T}"/> to get the best item. If <paramref name="comparator"/> returns true, it will become the item to be compared against.
    /// The first argument is the item in the list. The second argument is the item being compared
    /// </summary>
    /// <returns>The best item that satisfies <paramref name="comparator"/></returns>
    public static T GetBest<T> (this IEnumerable<T> values, Func<T, T, bool> comparator) {
        IEnumerator<T> enumerator = values.GetEnumerator ();
        enumerator.Reset ();
        enumerator.MoveNext (); // Starts enumeration
        
        T result = enumerator.Current;
        while (enumerator.MoveNext ())
            if (comparator (enumerator.Current, result))
                result = enumerator.Current;

        return result;
    }
}