using UnityEngine;
using System.IO;

public abstract class PersistentScriptableObject : ScriptableObject
{
    public virtual void Save(string filename)
    {
        string path = Application.dataPath;
        string json = JsonUtility.ToJson(this, true);
        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(Path.Combine(path, filename), json);
    }

    public virtual void Load(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}