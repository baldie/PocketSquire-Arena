#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public interface ILevelUpModel
    {
        int AvailablePoints { get; }
        List<string> PendingPerkChoices { get; }
        int CurrentLevel { get; }

        int GetAttributeValue(string attributeKey);
        int GetStartingAttributeValue(string attributeKey);
        void IncrementAttribute(string attributeKey);
        void DecrementAttribute(string attributeKey);
        
        List<Perk> GetEligiblePerks(List<Perk> pool);
        void SelectPerk(string perkId);
        bool IsPerkUnlocked(string perkId);
        void UnlockPerk(string perkId); // For unlocking perks directly (e.g. forced or from schedule)
        void SetPendingPerkChoices(List<string> perkIds);

        event Action? OnStatsChanged;
        event Action? OnPerksChanged;
    }
}
