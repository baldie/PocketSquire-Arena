namespace PocketSquire.Arena.Core.Perks
{
    /// <summary>
    /// Runtime (non-serialised) state for one active arena perk.
    /// Rebuilt from Player.ActiveArenaPerkIds on game load.
    /// </summary>
    public class ArenaPerkState
    {
        public string PerkId { get; set; } = string.Empty;
        public int CurrentStacks { get; set; }
        public int RemainingDuration { get; set; }
        public bool HasTriggeredThisBattle { get; set; }
        public bool HasTriggeredThisRun { get; set; }
        public int ConsecutiveCounter { get; set; }
        public bool ConsumedThisBattle { get; set; }

        public void ResetForBattle()
        {
            CurrentStacks = 0;
            RemainingDuration = 0;
            HasTriggeredThisBattle = false;
            ConsecutiveCounter = 0;
            ConsumedThisBattle = false;
        }

        public void ResetForRun()
        {
            ResetForBattle();
            HasTriggeredThisRun = false;
        }
    }
}
