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
        public static List<Monster> Monsters { get; set; } = new List<Monster>();
        public static List<Player> Players { get; set; } = new List<Player>();
        public static Battle? Battle { get; set; } = null;
        public static ProgressionLogic? Progression { get; set; }

        public static void Load(string? rootPath = null)
        {
            LoadMonsters(rootPath);
            LoadPlayers(rootPath);
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

                Monsters.Clear();

                if (monsters != null)
                {
                    Monsters.AddRange(monsters);
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

        public static Player? GetPlayerByName(string name)
        {
            if (Players == null) return null;
            return Players.Find(e => e.Name == name);
        }

        public static Monster? GetMonsterByName(string name)
        {
            if (Monsters == null) return null;
            return Monsters.Find(e => e.Name == name);
        }

        public static void ResetMonsters()
        {
            foreach (var monster in Monsters)
            {
                monster.Health = monster.MaxHealth;
            }
        }
    }
}