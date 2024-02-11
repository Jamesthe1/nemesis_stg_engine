using System.Collections.Generic;
using Godot;

public interface ISaveState<T> {
    static Dictionary<NodePath, T> States { get; set; }
    void SaveState ();
    void LoadState ();
}