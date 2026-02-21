#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PocketSquire.Arena.Core.LevelUp;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// This is the object that has the default settings of everything in the game
    /// </summary>
    public static class GameWorld
    {
        public static List<Monster> AllMonsters { get; set; } = new List<Monster>();
        public static List<Player> ClassTemplates { get; set; } = new List<Player>();
        public static List<Item> Items { get; set; } = new List<Item>();
        public static ProgressionLogic? Progression { get; set; }
        public static Dictionary<string, PerkPool> PerkPools { get; set; } = new Dictionary<string, PerkPool>();

        public static void Load(string? rootPath = null)
        {
            LoadMonsters(rootPath);
            LoadClassTemplates(rootPath);
            LoadItems(rootPath);
        }

        private static void LoadClassTemplates(string? rootPath = null)
        {
            // Load player information from files
            string root = rootPath ?? Environment.CurrentDirectory;
            string filePath = Path.Combine(root, "Assets/_Game/Data/classes.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Class data file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            try
            {
                var classTemplates = JsonConvert.DeserializeObject<List<Player>>(jsonContent);

                ClassTemplates.Clear();

                if (classTemplates != null)
                {
                    ClassTemplates.AddRange(classTemplates);
                }
            }
            catch (Exception ex)
            {
                // Core logic is framework agnostic, so we use Console or just throw.
                // The Unity side can catch and log to Debug if needed.
                Console.WriteLine($"Error loading class templates: {ex.Message}");
                throw;
            }   
        }

        private static void LoadMonsters(string? rootPath = null)
        {
            // Load monster information from files
            string root = rootPath ?? Environment.CurrentDirectory;
            string filePath = Path.Combine(root, "Assets/_Game/Data/monsters.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Monster data file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            try
            {
                var monsters = JsonConvert.DeserializeObject<List<Monster>>(jsonContent);

                AllMonsters.Clear();

                if (monsters != null)
                {
                    AllMonsters.AddRange(monsters);
                }
            }
            catch (Exception ex)
            {
                // Core logic is framework agnostic, so we use Console or just throw.
                // The Unity side can catch and log to Debug if needed.
                Console.WriteLine($"Error loading monsters: {ex.Message}");
                throw;
            }   
        }

        private static void LoadItems(string? rootPath = null)
        {
            try
            {
                string root = rootPath ?? Environment.CurrentDirectory;
                string filePath = Path.Combine(root, "Assets/_Game/Data/items.json");

                if (!File.Exists(filePath))
                {
                    // Items are critical, but maybe not present yet. Log and return or throw based on preference.
                    // For now, let's treat it as non-fatal but log it, or init an empty list.
                    // Actually, let's follow the pattern and throw/catch.
                    // But maybe items.json doesn't exist yet for existing projects.
                    // Let's assume it should exist if we are supporting items.
                    if (!File.Exists(filePath)) 
                    {
                         // If file missing, just empty list is fine for now to avoid breaking existing setups without it
                         // But we created items.json in previous turns.
                         throw new FileNotFoundException($"Item data file not found at: {filePath}");
                    }
                }

                string jsonContent = File.ReadAllText(filePath);
                var items = JsonConvert.DeserializeObject<List<Item>>(jsonContent);
                Items.Clear();
                if (items != null)
                {
                    Items.AddRange(items);
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Error loading items: {ex.Message}");
                 // throw; // Optional: rethrow if critical
            }
        }

        public static Player? GetClassTemplate(Player.Genders gender, PlayerClass.ClassName className)
        {
            if (ClassTemplates == null) return null;
            var genderStr = gender == Player.Genders.m ? "m" : "f";
            var targetName = $"{genderStr}_{className.ToString().ToLower()}";
            return ClassTemplates.Find(p => p.Name.ToLower() == targetName);
        }

        public static Monster? GetMonsterByName(string name)
        {
            if (AllMonsters == null) return null;
            return AllMonsters.Find(e => e.Name == name);
        }

        public static Item? GetItemById(int id)
        {
            return Items.Find(i => i.Id == id);
        }

        public static void ResetAllMonsters()
        {
            foreach (var monster in AllMonsters)
            {
                monster.Health = monster.MaxHealth;
            }
        }
    }
}