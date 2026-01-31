#nullable enable
using System;
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
            Player = GameWorld.GetPlayerByName("player_m_l1");
            
            // Give starting items (2 health potions, id=1)
            Player?.Inventory.AddItem(1, 2);
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