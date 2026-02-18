using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "GameAssetRegistry", menuName = "PocketSquire/GameAssetRegistry")]
public class GameAssetRegistry : ScriptableObject {
    private static GameAssetRegistry _instance;
    public static GameAssetRegistry Instance {
        get {
            if (_instance == null) {
                _instance = Resources.Load<GameAssetRegistry>("GameAssetRegistry");
            }
            return _instance;
        }
    }
    
    [Header("Visuals")]
    public List<SpriteEntry> sprites;

    [Header("Audio")]
    public List<AudioEntry> sounds;

    [Header("Prefabs")]
    public List<PrefabEntry> prefabs;
    public GameObject itemRowPrefab;

    [Header("Global Data")]
    public PocketSquire.Arena.Unity.LevelUp.ProgressionSchedule progressionSchedule;

    // Helper classes
    [System.Serializable] public struct SpriteEntry { public string id; public Sprite asset; }
    [System.Serializable] public struct AudioEntry { public string id; public AudioClip asset; }
    [System.Serializable] public struct PrefabEntry { public string id; public GameObject asset; }

    // Fast lookup caches
    private Dictionary<string, Sprite> _spriteCache;
    private Dictionary<string, AudioClip> _audioCache;
    private Dictionary<string, GameObject> _prefabCache;

    public Sprite GetSprite(string id) {
        _spriteCache ??= sprites.ToDictionary(x => x.id, x => x.asset);
        return _spriteCache.GetValueOrDefault(id);
    }

    public void LogAllSprites() {
        Debug.Log("SpritesCount: " + sprites.Count);
        foreach (var sprite in sprites) {
            Debug.Log($"{sprite.id}: {sprite.asset}");
        }
    }

    public AudioClip GetSound(string id) {
        _audioCache ??= sounds.ToDictionary(x => x.id, x => x.asset);
        return _audioCache.GetValueOrDefault(id);
    }

    public GameObject GetPrefab(string id) {
        _prefabCache ??= prefabs.ToDictionary(x => x.id, x => x.asset);
        return _prefabCache.GetValueOrDefault(id);
    }

#if UNITY_EDITOR
    [ContextMenu("Populate All Assets")]
    public void PopulateAll()
    {
        PopulateSprites();
        PopulateAudio();
        PopulatePrefabs();
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

    [ContextMenu("Populate Prefabs")]
    public void PopulatePrefabs()
    {
        prefabs ??= new List<PrefabEntry>();
        prefabs.Clear();

        string[] searchFolders = { "Assets/_Game/Prefabs" };
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", searchFolders);
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                prefabs.Add(new PrefabEntry { id = prefab.name, asset = prefab });
                if (prefab.name == "ItemRow") itemRowPrefab = prefab;
            }
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Populated {prefabs.Count} prefabs from Assets/_Game/Prefabs");
    }

    [UnityEditor.MenuItem("Tools/Populate Asset Registry")]
    public static void PopulateRegistryStatic()
    {
        // Try to find the one in Resources first, as that's what's used at runtime
        GameAssetRegistry registry = Resources.Load<GameAssetRegistry>("GameAssetRegistry");
        
        if (registry == null)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:GameAssetRegistry");
            if (guids.Length == 0) 
            {
                Debug.LogError("Could not find any GameAssetRegistry asset. Please create one in a Resources folder.");
                return;
            }

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            registry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameAssetRegistry>(path);
            
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Multiple GameAssetRegistry assets found. Populating the first one found at: {path}. " +
                                 "Consider deleting redundant registries.");
            }
        }
        
        if (registry != null)
        {
             registry.PopulateAll();
             Debug.Log($"Successfully populated GameAssetRegistry at {UnityEditor.AssetDatabase.GetAssetPath(registry)}");
        }
    }
#endif
}