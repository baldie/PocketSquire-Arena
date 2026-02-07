#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core.Buffs;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// This is the object that has the default settings of everything in the game
    /// </summary>
    public static class GameWorld
    {
        public static List<Monster> AllMonsters { get; set; } = new List<Monster>();
        public static List<Player> Players { get; set; } = new List<Player>();
        public static List<Item> Items { get; set; } = new List<Item>(); // Added Items list
        public static List<Buff> AllBuffs { get; set; } = new List<Buff>();
        public static ProgressionLogic? Progression { get; set; }

        public static void Load(string? rootPath = null)
        {
            LoadMonsters(rootPath);
            LoadPlayers(rootPath);
            LoadItems(rootPath); // Load items
            LoadBuffs(rootPath); // Load buffs
        }

        private static void LoadPlayers(string? rootPath = null)
        {
            // Load player information from files
            string root = rootPath ?? Environment.CurrentDirectory;
            string filePath = Path.Combine(root, "Assets/_Game/Data/players.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Player data file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            try
            {
                var players = JsonConvert.DeserializeObject<List<Player>>(jsonContent);

                Players.Clear();

                if (players != null)
                {
                    Players.AddRange(players);
                }
            }
            catch (Exception ex)
            {
                // Core logic is framework agnostic, so we use Console or just throw.
                // The Unity side can catch and log to Debug if needed.
                Console.WriteLine($"Error loading players: {ex.Message}");
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

        public static Player? GetPlayerByName(string name)
        {
            if (Players == null) return null;
            return Players.Find(e => e.Name == name);
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

        private static void LoadBuffs(string? rootPath = null)
        {
            try
            {
                string root = rootPath ?? Environment.CurrentDirectory;
                string filePath = Path.Combine(root, "Assets/_Game/Data/buffs.json");

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Buff data file not found at: {filePath}");
                }

                string jsonContent = File.ReadAllText(filePath);
                var buffArray = JArray.Parse(jsonContent);

                AllBuffs.Clear();

                foreach (var buffToken in buffArray)
                {
                    if (buffToken is JObject buffObj)
                    {
                        string id = buffObj["id"]?.ToString() ?? string.Empty;
                        string name = buffObj["name"]?.ToString() ?? string.Empty;
                        float duration = buffObj["duration"]?.Value<float>() ?? 0f;

                        var buff = new Buff(id, name, duration);

                        var componentsArray = buffObj["components"] as JArray;
                        if (componentsArray != null)
                        {
                            foreach (var componentToken in componentsArray)
                            {
                                if (componentToken is JObject componentObj)
                                {
                                    var component = BuffComponentFactory.CreateComponent(componentObj);
                                    buff.Components.Add(component);
                                }
                            }
                        }

                        AllBuffs.Add(buff);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading buffs: {ex.Message}");
                throw;
            }
        }

        public static Buff? GetBuffById(string id)
        {
            return AllBuffs.Find(b => b.Id == id);
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