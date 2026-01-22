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

        GameWorld.Battle = new Battle(LoadPlayer(GameState.Player), LoadMonster("Training Dummy"));
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
            if (battleMenu == null) return;
            
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

    private Player LoadPlayer(Player player)
    {
        if (player == null) return null;
        
        var playerSprite = GameObject.Find("PlayerSprite");
        if (playerSprite == null) return null;

        var playerImage = playerSprite.GetComponent<Image>();
        if (playerImage == null) return null;

        var loadedSprite = registry.GetSprite(player.GetSpriteId(Entity.GameContext.Battle));
        if (loadedSprite != null)
        {
            playerImage.overrideSprite = loadedSprite; 
        }
        else
        {
            Debug.LogError($"Sprite with ID {player.GetSpriteId(Entity.GameContext.Battle)} not found in registry!");
        }

        var rectTransform = playerSprite.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(player.PosX, player.PosY);
            rectTransform.sizeDelta = new Vector2(player.Width, player.Height);
            rectTransform.localScale = new Vector3(player.ScaleX, player.ScaleY, 1f);
        }
        else
        {
            Debug.LogError("PlayerSprite does not have a RectTransform!");
        }

        return player;
    }

    private Monster LoadMonster(string name)
    {
        var monster = GameWorld.GetMonsterByName(name);
        if (monster == null)
        {
            Debug.LogError($"{name} not found!");
            return null;
        }

        var monsterSprite = GameObject.Find("MonsterSprite");
        if (monsterSprite == null)
        {
            Debug.LogError("MonsterSprite game object not found!");
            return null;
        }

        var monsterImage = monsterSprite.GetComponent<Image>();
        if (monsterImage == null)
        {
            Debug.LogError("MonsterSprite image component not found!");
            return null;
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

        return monster;
    }
}
