using System;
using PocketSquire.Arena.Core;

[Serializable]
public class SaveData
{
    public SaveSlots SelectedSaveSlot;
    public DateTime CharacterCreationDate;
    public DateTime LastSaveDate;
    public TimeSpan PlayTime;
}