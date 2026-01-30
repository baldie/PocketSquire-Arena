namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Enumeration of all possible game action types.
    /// Used to identify the type of action for visual/audio lookups.
    /// </summary>
    public enum ActionType
    {
        Attack,
        SpecialAttack,
        Defend,
        Item,
        Hit,
        Defeat,
        Yield,
        Win,
        Lose,
        ChangeTurns
    }
}
