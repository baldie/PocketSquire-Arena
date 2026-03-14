#nullable enable
using PocketSquire.Arena.Core.PowerUps;

namespace PocketSquire.Arena.Core
{
    public static class CombatUtilities
    {
        public static Attributes GetEffectiveAttributes(Entity entity)
        {
            if (entity is Player player && GameState.CurrentRun != null)
            {
                var withPowerUps = new PlayerWithPowerUps(
                    player,
                    GameState.CurrentRun.PowerUps,
                    GameState.CurrentRun.ArenaRank);
                return withPowerUps.EffectiveAttributes;
            }

            return entity.Attributes;
        }
    }
}
