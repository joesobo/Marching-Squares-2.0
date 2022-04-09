using System;

[Serializable]
public class WorldSaveData {
    public string name;
    public int seed;
    public string last_played;

    public WorldSaveData(string name, int seed, string last_played) {
        this.name = name;
        this.seed = seed;
        this.last_played = last_played;
    }
}
