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

        public static void CreateNewGame(SaveSlots slot)
        {
            SelectedSaveSlot = slot;
            CharacterCreationDate = DateTime.Now;
            PlayTime = TimeSpan.Zero;
            LastSaveDate = DateTime.Now;
        }

        public static SaveData GetSaveData()
        {
            return new SaveData
            {
                SelectedSaveSlot = SelectedSaveSlot,
                CharacterCreationDate = CharacterCreationDate?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                LastSaveDate = LastSaveDate?.ToString(System.Globalization.CultureInfo.InvariantCulture),
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
            
            if (DateTime.TryParse(data.LastSaveDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime lastSaveDate))
                LastSaveDate = lastSaveDate;

            PlayTime = TimeSpan.FromTicks(data.PlayTimeTicks);
            Player = data.Player;
        }

        public static SaveData? FindMostRecentSave(SaveData?[]? saves)
        {
            if (saves == null || saves.Length == 0) return null;

            SaveData? mostRecentSave = null;
            DateTime mostRecentDate = DateTime.MinValue; 

            foreach (var save in saves)
            {
                if (save == null || string.IsNullOrEmpty(save.LastSaveDate)) continue;

                if (DateTime.TryParse(save.LastSaveDate, 
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