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

        public static void CreateNewGame(SaveSlots slot)
        {
            SelectedSaveSlot = slot;
            CharacterCreationDate = DateTime.Now;
            PlayTime = new TimeSpan(0, 0, 0, 0, 0);
            LastSaveDate = DateTime.Now;
        }

        public static SaveData GetSaveData()
        {
            return new SaveData
            {
                SelectedSaveSlot = SelectedSaveSlot,
                CharacterCreationDate = CharacterCreationDate.Value,
                LastSaveDate = LastSaveDate.Value,
                PlayTime = PlayTime.Value
            };   
        }
    }
}