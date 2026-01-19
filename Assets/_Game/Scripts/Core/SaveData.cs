using System;
using PocketSquire.Arena.Core;

[Serializable]
public class SaveData
{
    public SaveSlots SelectedSaveSlot;
    public DateTime CharacterCreationDate;
    public string LastSaveDateString;
    public TimeSpan PlayTime;
}