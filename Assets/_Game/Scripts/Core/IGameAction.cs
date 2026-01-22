using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents an action that can be queued and executed in the battle system.
    /// The 'Type' field can be used by the Unity layer to determine visuals.
    /// </summary>
    public interface IGameAction
    {
        /// <summary>
        /// The type of action, e.g., Attack, Block, UseItem.
        /// Used by the Unity layer to look up visual/audio assets.
        /// </summary>
        ActionType Type { get; }

        /// <summary>
        /// The entity performing the action.
        /// </summary>
        Entity Actor { get; }

        /// <summary>
        /// The entity receiving the action.
        /// </summary>
        Entity Target { get; }

        /// <summary>
        /// Applies the game-state changes for this action.
        /// Called by the Unity layer AFTER visuals are complete.
        /// </summary>
        void ApplyEffect();
    }
}
