using System;
using UnityEngine;

[Serializable]
public class WorldSaveData {
    public string name;
    public int seed;
    public string last_played;
    public Vector2 player_pos;

    public WorldSaveData(string name, int seed, string last_played, Vector2 player_pos) {
        this.name = name;
        this.seed = seed;
        this.last_played = last_played;
        this.player_pos = player_pos;
    }
}
