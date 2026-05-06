using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Kkyakdugi/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string       key;
        public AudioClip    clip;  
        public string       fullname;
    }

    [SerializeField] private Entry[] _entries;

    public AudioClip GetClip(string key)
    {
        foreach (var e in _entries)
            if (e.key == key) return e.clip;
        Debug.LogWarning($"[SoundDatabase] Clip not found: {key}");
        return null;
    }

    public string GetName(string key) {
        foreach (var e in _entries)
            if (e.key == key) return e.fullname;
        Debug.LogWarning($"[SoundDatabase] Clip not found: {key}");
        return null;
    }
}
