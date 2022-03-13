using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SerializationManager {
    public static bool Save(string path, string saveName, object saveData) {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        string fullPath = path + saveName + ".save";

        FileStream file = new FileStream(fullPath, FileMode.Create);
        formatter.Serialize(file, saveData);

        file.Close();
        return true;
    }

    public static object Load(string path) {
        if (!File.Exists(path)) {
            return null;
        }

        BinaryFormatter formatter = GetBinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try {
            object saveData = formatter.Deserialize(file);
            file.Close();
            return saveData;
        } catch {
            Debug.LogErrorFormat("Failed to load file at {0}", path);
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();

        SurrogateSelector selector = new SurrogateSelector();

        Vector3SerializationSurrogate vector3Surrogate = new Vector3SerializationSurrogate();
        Vector2SerializationSurrogate vector2Surrogate = new Vector2SerializationSurrogate();

        selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
        selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2Surrogate);

        formatter.SurrogateSelector = selector;

        return formatter;
    }
}
