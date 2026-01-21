using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using Newtonsoft.Json;
#endif

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// This is the object that has the default settings of everything in the game
    /// </summary>
    public static class GameWorld
    {
        public static List<Monster> Monsters { get; set; } = new List<Monster>();

        public static void Load(string? rootPath = null)
        {
            LoadMonsters(rootPath);
        }

        private static void LoadMonsters(string? rootPath = null)
        {
         // Load monster information from files
            string root = rootPath ?? Environment.CurrentDirectory;
            string filePath = Path.Combine(root, "Assets/_Game/Scripts/Data/monsters.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Monster data file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            try
            {
#if UNITY_5_3_OR_NEWER
                // JsonUtility doesn't support top-level arrays, so we wrap it
                string wrappedJson = "{\"monsters\":" + jsonContent + "}";
                var wrapper = JsonUtility.FromJson<MonsterListWrapper>(wrappedJson);

                Monsters.Clear();

                if (wrapper != null && wrapper.monsters != null)
                {
                    Monsters.AddRange(wrapper.monsters);
                }
#else
                // Fallback for unit tests where UnityEngine is not available
                var monsters = JsonConvert.DeserializeObject<List<Monster>>(jsonContent);

                Monsters.Clear();

                if (monsters != null)
                {
                    Monsters.AddRange(monsters);
                }
#endif
            }
            catch (Exception ex)
            {
#if UNITY_5_3_OR_NEWER
                Debug.LogError($"Error loading monsters: {ex.Message}");
#else
                Console.WriteLine($"Error loading monsters: {ex.Message}");
#endif
                throw;
            }   
        }

#if UNITY_5_3_OR_NEWER
        [Serializable]
        private class MonsterListWrapper
        {
            public List<Monster> monsters;
        }
#endif

        public static Monster? GetMonsterByName(string name)
        {
            if (Monsters == null) return null;
            return Monsters.Find(e => e.Name == name);
        }
    }
}