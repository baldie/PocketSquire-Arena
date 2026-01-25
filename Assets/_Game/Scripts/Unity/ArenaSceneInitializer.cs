using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

public class ArenaSceneInitializer : MonoBehaviour
{
    public GameAssetRegistry registry;
    
    [Header("Action Queue")]
    [Tooltip("Reference to the ActionQueueProcessor in the scene")]
    public ActionQueueProcessor actionQueueProcessor;

    [Header("Battle")]
    [Tooltip("Reference to the BattleManager in the scene")]
    public PocketSquire.Unity.BattleManager battleManager;
    
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


    // Update is called once per frame
    void Update()
    {
        #region Hide/Show Battle Menu
        if (GameWorld.Battle == null) return;
        if (GameWorld.Battle.CurrentTurn == null) return;

        var battleMenu = GameObject.Find("BattleMenuPanel");
        if (battleMenu == null) return;

        var battleMenuCanvas = battleMenu.GetComponent<Canvas>();
        if (battleMenuCanvas == null)
        {
            Debug.LogError("BattleMenu does not have a Canvas!");
            return;
        }
        
        bool shouldShowMenu = GameWorld.Battle.CurrentTurn.IsPlayerTurn && 
                             (actionQueueProcessor == null || (!actionQueueProcessor.IsProcessing && actionQueueProcessor.QueueCount == 0));

        battleMenuCanvas.enabled = shouldShowMenu;
        #endregion

        // Use the action queue for monster turn if processor is available
        if (GameWorld.Battle.CurrentTurn.IsPlayerTurn == false)
        {
            if (actionQueueProcessor != null && !actionQueueProcessor.IsProcessing && actionQueueProcessor.QueueCount == 0)
            {
                // Create and enqueue the monster's attack action
                var monster = GameWorld.Battle.CurrentTurn.Actor;
                var target = GameWorld.Battle.CurrentTurn.Target;
                
                if (monster != null && target != null)
                {
                    switch(monster.DetermineAction(target)) {
                        case ActionType.Attack:
                            battleManager.Attack();
                            break;
                        case ActionType.Defend:
                            battleManager.Defend();
                            break;
                        case ActionType.Yield:
                            battleManager.Yield();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.LogError("Monster or target is null!");
                }
            }
            else if (actionQueueProcessor == null)
            {
                // Fallback to old behavior if no processor assigned
                GameWorld.Battle.CurrentTurn.Execute();
            }
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
            playerImage.sprite = loadedSprite;
            playerImage.material = new Material(playerImage.material); // create a copy so its not shared
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
            monsterImage.sprite = loadedSprite; 
            monsterImage.material = new Material(monsterImage.material); // create a copy so its not shared
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
        if (GameWorld.Battle == null) return;

        // Check for battle end
        if (GameWorld.Battle.IsOver)
        {
            var winner = GameWorld.Battle.Player1.IsDead ? GameWorld.Battle.Player2 : GameWorld.Battle.Player1;
            Debug.Log($"Battle Over! Winner: {winner.Name}");
            // Use IsPlayerTurn based on who won? Or just stop.
            // If the player died, game over screen.
            // If monster died, victory screen.
            return;
        }

        // After an action completes, if battle continues, end the current turn
        if (GameWorld.Battle.CurrentTurn != null)
        {
            GameWorld.Battle.CurrentTurn.End();
        }
    }
}
