using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PocketSquire.Arena.Core;
using System.Collections;

public class ArenaSceneInitializer : MonoBehaviour
{
    // registry field removed - now using GameAssetRegistry.Instance
    
    [Header("Action Queue")]
    [Tooltip("Reference to the ActionQueueProcessor in the scene")]
    public ActionQueueProcessor actionQueueProcessor;

    [Header("Power-Up HUD")]
    [Tooltip("Reference to the PowerUpHudController in the scene")]
    public PowerUpHudController powerUpHud;

    [Header("Buttons")]
    public Button nextOpponentButton;
    public Button leaveArenaButton;
    
    void Start()
    {
        // Ensure GameWorld is loaded and we have a player - this allows us to start immediately in the arena
        if (GameWorld.AllMonsters.Count == 0 || GameState.Player == null)
        {
            if (GameWorld.AllMonsters.Count == 0) GameWorld.Load();
            if (GameState.Player == null) GameState.CreateNewGame(SaveSlots.Unknown);
        }

        // Ensure progression logic is loaded
        if (GameWorld.Progression == null && GameAssetRegistry.Instance.progressionSchedule != null)
        {
            GameWorld.Progression = GameAssetRegistry.Instance.progressionSchedule.Logic;
            GameWorld.PerkPools = GameAssetRegistry.Instance.progressionSchedule.RuntimePerkPools;
        }

        // Here we go!
        if (GameState.CurrentRun == null || GameState.CurrentRun.State == Run.RunState.NoStarted) {
            GameState.CurrentRun = Run.StartNewRun();
        }
        var monster = GameState.CurrentRun.GetMonsterForCurrentRank();
        GameState.Battle = new Battle(LoadPlayer(GameState.Player), monster);
        LoadMonster(monster);

        // Save the game (skip if testing with Unknown slot)
        if (GameState.SelectedSaveSlot != SaveSlots.Unknown)
        {
            SaveSystem.SaveGame(GameState.SelectedSaveSlot);
        }
        else
        {
            Debug.Log("[ArenaSceneInitializer] Skipping save for testing mode (Unknown slot)");
        }
        
        // Subscribe to action completion to handle turn changes
        if (actionQueueProcessor != null)
        {
            actionQueueProcessor.OnActionComplete += GameState.Battle.DetermineNextAction;
        }

        if (nextOpponentButton != null)
        {
            nextOpponentButton.onClick.AddListener(() => GoToScene("Arena", nextOpponentButton.gameObject));
        }

        if (leaveArenaButton != null)
        {
            leaveArenaButton.onClick.AddListener(() => GoToScene("Town", leaveArenaButton.gameObject));
        }

        // Populate power-up HUD
        if (powerUpHud != null && GameState.CurrentRun?.PowerUps != null)
        {
            powerUpHud.PopulateHud(GameState.CurrentRun.PowerUps);
        }
    }

    // Update is called once per frame
    // Removed polling: visibility is now handled explicitly via BattleManager.ShowMenu() event subscriptions.

    private Player LoadPlayer(Player player)
    {
        if (player == null) return null;
        
        var playerSprite = GameObject.Find("PlayerSprite");
        if (playerSprite == null) return null;

        var playerImage = playerSprite.GetComponent<Image>();
        if (playerImage == null) return null;

        var loadedSprite = GameAssetRegistry.Instance.GetSprite(player.GetSpriteId(Entity.GameContext.Battle));
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

    private Monster LoadMonster(Monster monster)
    {
        if (monster == null) return null;

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

        var loadedSprite = GameAssetRegistry.Instance.GetSprite(monster.SpriteId);
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
            actionQueueProcessor.OnActionComplete -= GameState.Battle.DetermineNextAction;
        }
    }

    public void GoToScene(string sceneName, GameObject buttonObj)
    {
        StartCoroutine(PlaySoundThenLoad(sceneName, buttonObj));
    }

    private IEnumerator PlaySoundThenLoad(string sceneName, GameObject buttonObj)
    {
        if (buttonObj != null)
        {
            var menuButtonSound = buttonObj.GetComponent<MenuButtonSound>();
            if (menuButtonSound != null && menuButtonSound.clickSound != null)
            {
                menuButtonSound.PlayClick();
                yield return new WaitForSecondsRealtime(menuButtonSound.clickSound.length);
            }
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[ArenaSceneInitializer] Loading scene '{sceneName}'");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"[ArenaSceneInitializer] Scene '{sceneName}' could not be loaded. Please ensure it is in Build Settings.");
        }
    }
}
