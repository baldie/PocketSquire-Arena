namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents anything that can be purchased in the shop (items, perks, etc.).
    /// </summary>
    public interface IMerchandise
    {
        string DisplayName { get; }
        string Description { get; }
        int Price { get; }
    }
}
