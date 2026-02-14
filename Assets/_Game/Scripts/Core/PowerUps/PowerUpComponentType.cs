using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Categories of power-up components.
    /// </summary>
    [Serializable]
    public enum PowerUpComponentType
    {
        AttributeModifier,
        LootModifier,
        Utility,
        MonsterDebuff
    }
}
