namespace PocketSquire.Arena.Core.Town
{
    /// <summary>
    /// Actions available in the visual novel-style dialogue system.
    /// Used by LocationData to define what options are shown to the player.
    /// </summary>
    public enum DialogueAction
    {
        None,
        Leave,      // Return to town map
        Shop,       // Open shop interface
        Train,      // Open training interface
        Talk,       // Continue dialogue
        Prepare     // Prepare for adventure (home)
    }
}
