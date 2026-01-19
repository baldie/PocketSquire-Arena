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
        public static string LastSaveDateString = null;
        public static TimeSpan? PlayTime = null;

        public static void CreateNewGame(SaveSlots slot)
        {
            SelectedSaveSlot = slot;
            CharacterCreationDate = DateTime.Now;
            PlayTime = new TimeSpan(0, 0, 0, 0, 0);
            LastSaveDateString = DateTime.Now.ToString();
        }

        public static SaveData GetSaveData()
        {
            return new SaveData
            {
                SelectedSaveSlot = SelectedSaveSlot,
                CharacterCreationDate = CharacterCreationDate.Value,
                LastSaveDateString = LastSaveDateString,
                PlayTime = PlayTime.Value
            };   
        }

        public static void LoadFromSaveData(SaveData data)
        {
            if (data == null) return;
            
            SelectedSaveSlot = data.SelectedSaveSlot;
            CharacterCreationDate = data.CharacterCreationDate;
            LastSaveDateString = data.LastSaveDateString;
            PlayTime = data.PlayTime;
        }

        public static SaveData FindMostRecentSave(SaveData[] saves)
        {
            if (saves == null || saves.Length == 0) return null;

            SaveData mostRecentSave = null;
            // Track the actual DateTime object of the winner so we don't have to re-parse it constantly
            DateTime mostRecentDate = DateTime.MinValue; 

            foreach (var save in saves)
            {
                if (save == null || string.IsNullOrEmpty(save.LastSaveDateString)) continue;

                // 1. Convert string back to DateTime
                // Note: This relies on the string being a valid date format
                if (DateTime.TryParse(save.LastSaveDateString, out DateTime saveDate))
                {
                    // 2. Compare DateTimes directly
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