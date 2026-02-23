using System;
using Newtonsoft.Json;
namespace PocketSquire.Arena.Core
{
    public enum SaveSlots
    {
        Unknown,
        Slot1,
        Slot2,
        Slot3
    }

    public static class GameState
    {
        public static SaveSlots SelectedSaveSlot = SaveSlots.Unknown;
        public static DateTime? CharacterCreationDate = null;
        public static DateTime? LastSaveDate = null;
        public static TimeSpan? PlayTime = null;
        public static Player? Player = null;
        public static Run? CurrentRun = null;
        public static Battle? Battle { get; set; } = null;

        public static void CreateNewGame(SaveSlots slot)
        {
            SelectedSaveSlot = slot;
            CharacterCreationDate = DateTime.Now;
            PlayTime = TimeSpan.Zero;
            LastSaveDate = DateTime.Now;
            //TODO: character creation to select gender & starting attributes
            var prototype = GameWorld.GetClassTemplate(Player.Genders.m, PlayerClass.ClassName.Squire);
            if (prototype != null)
            {
                // Deep clone via JSON to avoid reference issues
                var json = JsonConvert.SerializeObject(prototype);
                Player = JsonConvert.DeserializeObject<Player>(json);
                if (Player != null){
                    Player.MaxHealth = 20;
                    Player.Health = 20;
                    Player.Gold = 100;
                    Player.Attributes = Attributes.GetDefaultAttributes();
                    // Give starting item: 1 Small Health Potion
                    Player.Inventory.AddItem(1, 1);
                }
            }
        }

        public static SaveData GetSaveData()
        {
            return new SaveData
            {
                SelectedSaveSlot = SelectedSaveSlot,
                CharacterCreationDate = CharacterCreationDate?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                LastSaveDateString = LastSaveDate?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                PlayTimeTicks = PlayTime?.Ticks ?? 0,
                Player = Player
            };   
        }

        public static void LoadFromSaveData(SaveData data)
        {
            if (data == null) return;
            
            SelectedSaveSlot = data.SelectedSaveSlot;
            
            if (DateTime.TryParse(data.CharacterCreationDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime creationDate))
                CharacterCreationDate = creationDate;
            
            if (DateTime.TryParse(data.LastSaveDateString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime lastSaveDate))
                LastSaveDate = lastSaveDate;

            PlayTime = TimeSpan.FromTicks(data.PlayTimeTicks);
            Player = data.Player;
            Console.WriteLine("Loaded Player: " + Player?.Name);
        }

        public static void AccumulatePlaytime(TimeSpan sessionDuration)
        {
            if (PlayTime == null)
                PlayTime = TimeSpan.Zero;
            
            PlayTime = PlayTime.Value.Add(sessionDuration);
        }

        public static SaveData? FindMostRecentSave(SaveData?[]? saves)
        {
            if (saves == null || saves.Length == 0) return null;

            SaveData? mostRecentSave = null;
            DateTime mostRecentDate = DateTime.MinValue; 

            foreach (var save in saves)
            {
                if (save == null || string.IsNullOrEmpty(save.LastSaveDateString)) continue;

                if (DateTime.TryParse(save.LastSaveDateString, 
                  System.Globalization.CultureInfo.InvariantCulture, 
                  System.Globalization.DateTimeStyles.None, 
                  out DateTime saveDate))
                {
                    if (mostRecentSave == null || saveDate > mostRecentDate)
                    {
                        mostRecentSave = save;
                        mostRecentDate = saveDate;
                    }
                }
            }
            return mostRecentSave;
        }
    }
}