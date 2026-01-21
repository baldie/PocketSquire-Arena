using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

public class ArenaSceneInitializer : MonoBehaviour
{
    public GameAssetRegistry registry;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If the game world is empty, load it - this allows us to start immediately in the arena
        if (GameWorld.Monsters.Count == 0)
        {
            GameWorld.Load();
            GameState.CreateNewGame(SaveSlots.Unknown);
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

        var loadedSprite = registry.GetSprite(monster.SpriteId);
        if (loadedSprite != null)
        {
            monsterImage.overrideSprite = loadedSprite; 
        }
        else
        {
            Debug.LogError($"Sprite with ID {monster.SpriteId} not found in registry!");
        }

        var rectTransform = monsterSprite.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(monster.PosX, monster.PosY);
            rectTransform.sizeDelta = new Vector2(monster.Width, monster.Height);
            rectTransform.localScale = new Vector3(monster.ScaleX, monster.ScaleY, 1f);
        }
        else
        {
            Debug.LogError("MonsterSprite does not have a RectTransform!");
        }
        #endregion

        GameWorld.Battle = new Battle(GameState.Player!, monster);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameWorld.Battle == null) return;
        if (GameWorld.Battle.CurrentTurn == null) return;
        
        if (GameWorld.Battle.CurrentTurn.IsPlayerTurn)
        {
            // Show battle menu
            var battleMenu = GameObject.Find("BattleMenu");
            if (battleMenu == null)
            {
                Debug.LogError("BattleMenu game object not found!");
                return;
            }
            
            var canvas = battleMenu.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("BattleMenu does not have a Canvas!");
                return;
            }
            
            if (!canvas.enabled) canvas.enabled = true;
        }
        else
        {
            // Hide battle menu
            var canvas = GameObject.Find("BattleMenu").GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("BattleMenu does not have a Canvas!");
                return;
            }
            
            if (canvas.enabled) canvas.enabled = false;

            GameWorld.Battle.CurrentTurn.Execute();
        }
    }
}
