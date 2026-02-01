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

#if UNITY_EDITOR
    [ContextMenu("Populate All Assets")]
    public void PopulateAll()
    {
        PopulateSprites();
        PopulateAudio();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    [ContextMenu("Populate Sprites")]
    public void PopulateSprites()
    {
        sprites ??= new List<SpriteEntry>();
        sprites.Clear();

        string[] searchFolders = { "Assets/_Game/Art" };
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", searchFolders);
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            
            if (sprite != null)
            {
                sprites.Add(new SpriteEntry { id = sprite.name, asset = sprite });
            }
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Populated {sprites.Count} sprites from Assets/_Game/Art");
    }

    [ContextMenu("Populate Audio")]
    public void PopulateAudio()
    {
        sounds ??= new List<AudioEntry>();
        sounds.Clear();

        string[] searchFolders = { "Assets/_Game/Audio" };
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", searchFolders);
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            if (clip != null)
            {
                sounds.Add(new AudioEntry { id = clip.name, asset = clip });
            }
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Populated {sounds.Count} audio clips from Assets/_Game/Audio");
    }

    [UnityEditor.MenuItem("Tools/Populate Asset Registry")]
    public static void PopulateRegistryStatic()
    {
        var guids = UnityEditor.AssetDatabase.FindAssets("t:GameAssetRegistry");
        if (guids.Length == 0) 
        {
            Debug.LogError("Could not find GameAssetRegistry asset");
            return;
        }

        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        var registry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameAssetRegistry>(path);
        
        if (registry != null)
        {
             registry.PopulateAll();
        }
    }
#endif
}