using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpeakerPortraitDatabase", menuName = "Game/Speaker Portrait Database")]
public class SpeakerPortraitDatabase : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string key;
        public Sprite sprite;
    }

    public Entry[] entries;

    private Dictionary<string, Sprite> _lookup;

    public Sprite GetSprite(string key)
    {
        if (_lookup == null)
            BuildLookup();

        if (string.IsNullOrEmpty(key)) return null;
        _lookup.TryGetValue(key, out var sprite);
        return sprite;
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<string, Sprite>(entries.Length);
        foreach (var e in entries)
            _lookup[e.key] = e.sprite;
    }

    private void OnValidate()
    {
        _lookup = null;
    }
}
