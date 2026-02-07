using System;

namespace PocketSquire.Arena.Core.Town
{
    /// <summary>
    /// Represents a single dialogue option shown to the player.
    /// Combines display text with the action to perform when selected.
    /// </summary>
    [Serializable]
    public struct DialogueOption
    {
        public string buttonText;
        public DialogueAction action;

        public DialogueOption(string buttonText, DialogueAction action)
        {
            this.buttonText = buttonText;
            this.action = action;
        }

        /// <summary>
        /// Creates a "Leave" dialogue option with standard text.
        /// </summary>
        public static DialogueOption Leave() => new DialogueOption("Leave", DialogueAction.Leave);

        /// <summary>
        /// Creates a "Shop" dialogue option with standard text.
        /// </summary>
        public static DialogueOption Shop() => new DialogueOption("Shop", DialogueAction.Shop);

        /// <summary>
        /// Creates a "Train" dialogue option with standard text.
        /// </summary>
        public static DialogueOption Train() => new DialogueOption("Train", DialogueAction.Train);

        /// <summary>
        /// Creates a "Prepare" dialogue option with standard text.
        /// </summary>
        public static DialogueOption Prepare() => new DialogueOption("Prepare", DialogueAction.Prepare);
    }
}
