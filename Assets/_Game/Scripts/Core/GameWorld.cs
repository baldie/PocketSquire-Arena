#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core.Perks;

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
        public static List<Perk> AllPerks { get; set; } = new List<Perk>();
        public static ProgressionLogic? Progression { get; set; }
        public static Dictionary<string, PerkPool> PerkPools { get; set; } = new Dictionary<string, PerkPool>();

        public static void Load(string? rootPath = null)
        {
            LoadMonsters(rootPath);
            LoadClassTemplates(rootPath);
            LoadItems(rootPath);
            LoadPerks(rootPath);
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
                    throw new FileNotFoundException($"Item data file not found at: {filePath}");
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
                monster.ResetForRun();
            }
        }

        private static void LoadPerks(string? rootPath = null)
        {
            string root = rootPath ?? Environment.CurrentDirectory;
            string filePath = Path.Combine(root, "Assets/_Game/Data/arena_perks.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arena perks data file not found at: {filePath}");

            string jsonContent = File.ReadAllText(filePath);
            try
            {
                var wrapper = JsonConvert.DeserializeObject<PerkWrapper>(jsonContent);
                AllPerks.Clear();
                if (wrapper?.Perks != null)
                    AllPerks.AddRange(wrapper.Perks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading arena perks: {ex.Message}");
                throw;
            }
        }

        public static Perk? GetPerkById(string id)
            => AllPerks.Find(p => p.Id == id);

        public static List<Perk> GetPerksByVendor(VendorType vendor)
            => AllPerks.FindAll(p => p.Vendor == vendor);
    }
}
