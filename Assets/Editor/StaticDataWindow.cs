using System.Linq;
using UnityEditor;
using UnityEngine;
using Alchemy.Editor;
using Alchemy.Inspector;
using PocketSquire.Arena.Core;

/// <summary>
/// Editor window for debugging GameState and GameWorld static instances in real-time.
/// Uses Alchemy to automatically render properties with proper grouping and formatting.
/// </summary>
public class StaticDataWindow : AlchemyEditorWindow
{
    [MenuItem("Window/Game Debugger")]
    static void Open()
    {
        var window = GetWindow<StaticDataWindow>("Game Debugger");
        window.Show();
    }

    // ========== GameState Fields ==========
    
    [FoldoutGroup("Game State"), ReadOnly]
    public SaveSlots selectedSaveSlot = SaveSlots.Unknown;
    
    [FoldoutGroup("Game State"), ReadOnly]
    public string characterCreationDate = "N/A";
    
    [FoldoutGroup("Game State"), ReadOnly]
    public string lastSaveDate = "N/A";
    
    [FoldoutGroup("Game State"), ReadOnly]
    public string playTime = "N/A";
    
    // Player data (simplified from complex Player object)
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public string playerName = "N/A";
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerLevel;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerHealth;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerMaxHealth;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerStrength;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerDefense;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerSpeed;
    
    [FoldoutGroup("Game State/Player"), ReadOnly]
    public int playerLuck;
    
    [FoldoutGroup("Game State/Player/Inventory"), ReadOnly]
    public int inventoryItemCount;
    
    [FoldoutGroup("Game State/Player/Inventory"), ReadOnly, TextArea]
    public string inventoryContents = "N/A";
    
    // Current Run data
    [FoldoutGroup("Game State/Current Run"), ReadOnly]
    public string currentRunStatus = "N/A";
    
    [FoldoutGroup("Game State/Current Run"), ReadOnly]
    public int runFloorNumber;
    
    // Battle data
    [FoldoutGroup("Game State/Battle"), ReadOnly]
    public string battleStatus = "N/A";
    
    [FoldoutGroup("Game State/Battle"), ReadOnly]
    public string currentMonster = "N/A";
    
    // ========== GameWorld Fields ==========
    
    [FoldoutGroup("Game World"), ReadOnly]
    public int monsterCount;
    
    [FoldoutGroup("Game World"), ReadOnly]
    public int playerDefinitionsCount;
    
    [FoldoutGroup("Game World"), ReadOnly]
    public int itemsCount;
    
    [FoldoutGroup("Game World/Monsters"), ReadOnly, TextArea(3, 10)]
    public string monsterNames = "";
    
    [FoldoutGroup("Game World/Items"), ReadOnly, TextArea(3, 10)]
    public string itemNames = "";
    
    [FoldoutGroup("Game World/Player Definitions"), ReadOnly, TextArea(3, 10)]
    public string playerDefinitions = "";

    // Update the window continuously while it's open
    private void OnInspectorUpdate()
    {
        UpdateGameStateData();
        UpdateGameWorldData();
        
        // Repaint the window to show real-time updates while the game is running
        Repaint();
    }

    private void UpdateGameStateData()
    {
        // Basic GameState properties
        selectedSaveSlot = GameState.SelectedSaveSlot;
        characterCreationDate = GameState.CharacterCreationDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        lastSaveDate = GameState.LastSaveDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        playTime = GameState.PlayTime?.ToString(@"hh\:mm\:ss") ?? "N/A";
        
        // Player data
        if (GameState.Player != null)
        {
            playerName = GameState.Player.Name ?? "N/A";
            playerLevel = GameState.Player.Level;
            playerHealth = GameState.Player.Health;
            playerMaxHealth = GameState.Player.MaxHealth;
            playerStrength = GameState.Player.Attributes.Strength;
            playerDefense = GameState.Player.Attributes.Defense;
            playerSpeed = 0; // Speed is not in Attributes, using 0
            playerLuck = GameState.Player.Attributes.Luck;
            
            // Inventory
            if (GameState.Player.Inventory != null)
            {
                inventoryItemCount = GameState.Player.Inventory.Slots.Count;
                inventoryContents = string.Join("\n", 
                    GameState.Player.Inventory.Slots.Select(slot => 
                    {
                        var item = GameWorld.GetItemById(slot.ItemId);
                        return $"{item?.Name ?? "Unknown"} (ID: {slot.ItemId}) x{slot.Quantity}";
                    }));
                
                if (string.IsNullOrEmpty(inventoryContents))
                {
                    inventoryContents = "Empty";
                }
            }
            else
            {
                inventoryItemCount = 0;
                inventoryContents = "N/A";
            }
        }
        else
        {
            playerName = "N/A";
            playerLevel = 0;
            playerHealth = 0;
            playerMaxHealth = 0;
            playerStrength = 0;
            playerDefense = 0;
            playerSpeed = 0;
            playerLuck = 0;
            inventoryItemCount = 0;
            inventoryContents = "N/A";
        }
        
        // Current Run data
        if (GameState.CurrentRun != null)
        {
            currentRunStatus = "Active";
            runFloorNumber = GameState.CurrentRun.ArenaRank;
        }
        else
        {
            currentRunStatus = "No active run";
            runFloorNumber = 0;
        }
        
        // Battle data
        if (GameState.Battle != null)
        {
            battleStatus = "In Battle";
            currentMonster = GameState.Battle.Player2?.Name ?? "Unknown";
        }
        else
        {
            battleStatus = "Not in battle";
            currentMonster = "N/A";
        }
    }

    private void UpdateGameWorldData()
    {
        // GameWorld counts
        monsterCount = GameWorld.AllMonsters?.Count ?? 0;
        playerDefinitionsCount = GameWorld.Players?.Count ?? 0;
        itemsCount = GameWorld.Items?.Count ?? 0;
        
        // Monster names
        if (GameWorld.AllMonsters != null && GameWorld.AllMonsters.Count > 0)
        {
            monsterNames = string.Join("\n", 
                GameWorld.AllMonsters.Select(m => 
                    $"{m?.Name ?? "Unknown"} (Rank {m?.Rank ?? 0}, HP: {m?.MaxHealth ?? 0})"));
        }
        else
        {
            monsterNames = "No monsters loaded";
        }
        
        // Item names
        if (GameWorld.Items != null && GameWorld.Items.Count > 0)
        {
            itemNames = string.Join("\n", 
                GameWorld.Items.Select(i => 
                    $"{i?.Name ?? "Unknown"} (ID: {i?.Id ?? 0})"));
        }
        else
        {
            itemNames = "No items loaded";
        }
        
        // Player definitions
        if (GameWorld.Players != null && GameWorld.Players.Count > 0)
        {
            playerDefinitions = string.Join("\n", 
                GameWorld.Players.Select(p => 
                    $"{p?.Name ?? "Unknown"} (Lvl {p?.Level ?? 0}, HP: {p?.MaxHealth ?? 0})"));
        }
        else
        {
            playerDefinitions = "No player definitions loaded";
        }
    }
}