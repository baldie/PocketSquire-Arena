using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "GameAssetRegistry", menuName = "Game/Asset Registry")]
public class GameAssetRegistry : ScriptableObject {
    
    [Header("Visuals")]
    public List<SpriteEntry> sprites;

    [Header("Audio")]
    public List<AudioEntry> sounds;

    // Helper classes
    [System.Serializable] public struct SpriteEntry { public string id; public Sprite asset; }
    [System.Serializable] public struct AudioEntry { public string id; public AudioClip asset; }

    // Fast lookup caches
    private Dictionary<string, Sprite> _spriteCache;
    private Dictionary<string, AudioClip> _audioCache;

    public Sprite GetSprite(string id) {
        _spriteCache ??= sprites.ToDictionary(x => x.id, x => x.asset);
        return _spriteCache.GetValueOrDefault(id);
    }

    public AudioClip GetSound(string id) {
        _audioCache ??= sounds.ToDictionary(x => x.id, x => x.asset);
        return _audioCache.GetValueOrDefault(id);
    }
}