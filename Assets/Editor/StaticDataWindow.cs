using System.Linq;
using System.Collections.Generic;
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

    // ========== Game State ==========
    
    [ReadOnly, ShowInInspector]
    public SaveSlots SelectedSaveSlot;
    
    [ReadOnly, ShowInInspector]
    public string CharacterCreationDate;
    
    [ReadOnly, ShowInInspector]
    public string LastSaveDate;
    
    [ReadOnly, ShowInInspector]
    public string PlayTime;
    
    [ReadOnly, ShowInInspector]
    public Player Player;
    
    [ReadOnly, ShowInInspector]
    public Run CurrentRun;
    
    [ReadOnly, ShowInInspector]
    public Battle Battle;
    
    // ========== Game World ==========
    
    [ReadOnly, ShowInInspector]
    public List<Monster> AllMonsters;
    
    [ReadOnly, ShowInInspector]
    public List<Player> PlayerDefinitions;
    
    [ReadOnly, ShowInInspector]
    public List<Item> Items;

    [ReadOnly, ShowInInspector]
    public int MonsterCount;

    [ReadOnly, ShowInInspector]
    public int PlayerDefinitionsCount;

    [ReadOnly, ShowInInspector]
    public int ItemsCount;

    // Update the window continuously while it's open
    private void OnInspectorUpdate()
    {
        // Game State Updates
        SelectedSaveSlot = GameState.SelectedSaveSlot;
        CharacterCreationDate = GameState.CharacterCreationDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        LastSaveDate = GameState.LastSaveDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        PlayTime = GameState.PlayTime?.ToString(@"hh\:mm\:ss") ?? "N/A";
        Player = GameState.Player;
        CurrentRun = GameState.CurrentRun;
        Battle = GameState.Battle;

        // Game World Updates
        AllMonsters = GameWorld.AllMonsters;
        PlayerDefinitions = GameWorld.Players;
        Items = GameWorld.Items;
        MonsterCount = GameWorld.AllMonsters?.Count ?? 0;
        PlayerDefinitionsCount = GameWorld.Players?.Count ?? 0;
        ItemsCount = GameWorld.Items?.Count ?? 0;

        // Repaint the window to show real-time updates while the game is running
        Repaint();
    }
}