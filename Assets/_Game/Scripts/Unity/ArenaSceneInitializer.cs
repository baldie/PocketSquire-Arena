using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

public class ArenaSceneInitializer : MonoBehaviour
{
    public GameAssetRegistry registry;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If the game world is empty, load it
        if (GameWorld.Monsters.Count == 0)
        {
            GameWorld.Load();
        }

        #region Load monster sprite
        var monster = GameWorld.GetMonsterByName("Training Dummy");
        if (monster == null)
        {
            Debug.LogError("Training Dummy not found!");
            return;
        }

        var monsterSprite = GameObject.Find("MonsterSprite");
        if (monsterSprite == null)
        {
            Debug.LogError("MonsterSprite game object not found!");
            return;
        }

        var monsterImage = monsterSprite.GetComponent<Image>();
        if (monsterImage == null)
        {
            Debug.LogError("MonsterSprite image component not found!");
            return;
        }

        Sprite loadedSprite = registry.GetSprite(monster.SpriteId);
        if (loadedSprite != null)
        {
            monsterImage.overrideSprite = loadedSprite; 
        }
        else
        {
            Debug.LogError($"Sprite with ID {monster.SpriteId} not found in registry!");
        }
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
