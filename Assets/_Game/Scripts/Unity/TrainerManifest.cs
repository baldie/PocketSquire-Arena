using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core;
using static PocketSquire.Arena.Core.Player;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Unity.LevelUp;

[CreateAssetMenu(fileName = "NewTrainerManifest", menuName = "PocketSquire/TrainerManifest")]
public class TrainerManifest : ScriptableObject
{
    public string TrainerName;
    [TextArea] public string welcomeMessage;

    [Header("Available Training")]
    [SerializeField] private List<TrainerProduct> trainingCatalog = new List<TrainerProduct>();

    [Serializable]
    public class TrainerProduct
    {
        public PerkNode perk;
        public int goldCost;
        
        // This allows you to hide "Sniper" perks until they are actually that class
        public PlayerClass requiredClass; 
    }

    // Helper to get only the perks relevant to the player's current evolution
    public List<TrainerProduct> GetAvailableTraining(PlayerClass playerClass)
    {
        return trainingCatalog
            .Where(item => item.requiredClass == playerClass)
            .ToList();
    }
}