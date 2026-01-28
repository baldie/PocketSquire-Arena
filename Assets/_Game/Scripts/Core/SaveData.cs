#nullable enable
using System;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class SaveData
    {
        public SaveSlots SelectedSaveSlot;
        public string? CharacterCreationDate;
        public string? LastSaveDateString;
        public long PlayTimeTicks;
        public Player? Player;
    }
}