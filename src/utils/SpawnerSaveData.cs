using Godot;

public struct SpawnerSaveData {
    public SpawnerDataResource data;
    public double timeTrigger;
    public int fireId;

    public SpawnerSaveData (SpawnerDataResource data, double timeTrigger, int fireId) {
        this.data = data;
        this.timeTrigger = timeTrigger;
        this.fireId = fireId;
    }
}