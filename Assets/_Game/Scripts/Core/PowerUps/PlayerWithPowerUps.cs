#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Wrapper class that computes effective player stats with power-ups applied,
    /// without mutating the base Player instance.
    /// </summary>
    public class PlayerWithPowerUps
    {
        public Player BasePlayer { get; private set; }
        public PowerUpCollection PowerUps { get; private set; }
        public int ArenaLevel { get; private set; }

        private Attributes? _cachedEffectiveAttributes;

        public PlayerWithPowerUps(Player basePlayer, PowerUpCollection powerUps, int arenaLevel)
        {
            BasePlayer = basePlayer ?? throw new ArgumentNullException(nameof(basePlayer));
            PowerUps = powerUps ?? throw new ArgumentNullException(nameof(powerUps));
            ArenaLevel = arenaLevel;
        }

        /// <summary>
        /// Returns the effective attributes with all power-up modifiers applied.
        /// Result is cached until power-ups or arena level change.
        /// </summary>
        public Attributes EffectiveAttributes
        {
            get
            {
                if (_cachedEffectiveAttributes == null)
                {
                    _cachedEffectiveAttributes = ComputeEffectiveAttributes();
                }
                return _cachedEffectiveAttributes;
            }
        }

        /// <summary>
        /// Invalidates the cached attributes (call when power-ups change).
        /// </summary>
        public void InvalidateCache()
        {
            _cachedEffectiveAttributes = null;
        }

        private Attributes ComputeEffectiveAttributes()
        {
            // Create a copy of base attributes
            var effective = new Attributes
            {
                Strength = BasePlayer.Attributes.Strength,
                Constitution = BasePlayer.Attributes.Constitution,
                Intelligence = BasePlayer.Attributes.Intelligence,
                Agility = BasePlayer.Attributes.Agility,
                Luck = BasePlayer.Attributes.Luck,
                Defense = BasePlayer.Attributes.Defense
            };

            // Apply all attribute modifiers
            foreach (var powerUp in PowerUps.GetAll())
            {
                if (powerUp.Component is AttributeModifierComponent modifier)
                {
                    modifier.ApplyToAttributes(effective, ArenaLevel);
                }
            }

            return effective;
        }
    }
}
