using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

public class ArenaSceneInitializer : MonoBehaviour
{
    public GameAssetRegistry registry;
    
    [Tooltip("Reference to the ActionQueueProcessor in the scene")]
    public ActionQueueProcessor actionQueueProcessor;
    
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
        
        // Subscribe to action completion to handle turn changes
        if (actionQueueProcessor != null)
        {
            actionQueueProcessor.OnActionComplete += HandleActionComplete;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (actionQueueProcessor != null)
        {
            actionQueueProcessor.OnActionComplete -= HandleActionComplete;
        }
    }
    
    private void HandleActionComplete(IGameAction action)
    {
        // After an action completes, end the current turn
        if (GameWorld.Battle?.CurrentTurn != null)
        {
            GameWorld.Battle.CurrentTurn.End();
        }
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

            // Use the action queue for monster turn if processor is available
            if (actionQueueProcessor != null && !actionQueueProcessor.IsProcessing && actionQueueProcessor.QueueCount == 0)
            {
                // Create and enqueue the monster's attack action
                var monster = GameWorld.Battle.CurrentTurn.IsPlayerTurn ? null : GetCurrentActor();
                var target = GameWorld.Battle.CurrentTurn.IsPlayerTurn ? null : GetCurrentTarget();
                
                if (monster != null && target != null)
                {
                    int damage = CalculateDamage(monster, target);
                    var attackAction = new AttackAction(monster, target, damage);
                    actionQueueProcessor.EnqueueAction(attackAction);
                }
            }
            else if (actionQueueProcessor == null)
            {
                // Fallback to old behavior if no processor assigned
                GameWorld.Battle.CurrentTurn.Execute();
            }
        }
    }
    
    private Entity GetCurrentActor()
    {
        // The monster is player2 in the battle when it's the monster's turn
        return GameWorld.Battle?.CurrentTurn?.IsPlayerTurn == false ? 
            GameWorld.GetMonsterByName("Training Dummy") : null;
    }
    
    private Entity GetCurrentTarget()
    {
        // The player is the target when it's the monster's turn
        return GameWorld.Battle?.CurrentTurn?.IsPlayerTurn == false ? 
            GameState.Player : null;
    }
    
    private int CalculateDamage(Entity attacker, Entity target)
    {
        // Basic damage calculation - can be made more complex later
        int baseDamage = attacker.Attributes.Strength;
        return Mathf.Max(1, baseDamage); // Minimum 1 damage
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
