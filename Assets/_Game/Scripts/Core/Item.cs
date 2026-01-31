using System;

namespace PocketSquire.Arena.Core
{
    public enum ItemTarget
    {
        Self,
        Enemy
    }

    [Serializable]
    public class Item
    {
        public int Id;
        public string Name = string.Empty;
        public string Description = string.Empty;
        public ItemTarget Target = ItemTarget.Self;
        public bool Stackable = true;
        public string Sprite = string.Empty;
        public string SoundEffect = string.Empty;
        public int Price;
    }
}
