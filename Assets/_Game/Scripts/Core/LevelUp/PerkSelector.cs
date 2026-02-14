#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.LevelUp
{
    public static class PerkSelector
    {
        public class SelectionContext
        {
            public int PlayerLevel { get; set; }
            public HashSet<string> UnlockedPerkIds { get; set; } = new HashSet<string>();
        }

        /// <summary>
        /// Selects up to 'count' eligible perks from the given pool.
        /// Selection is randomized using the provided RNG.
        /// </summary>
        public static List<Perk> Select(
            PerkPool pool, 
            int count,
            SelectionContext context,
            Random rng)
        {
            if (pool == null || pool.Perks == null || pool.Perks.Count == 0)
                return new List<Perk>();

            // 1. Filter ineligible perks
            var eligible = pool.Perks.Where(p => IsEligible(p, context)).ToList();

            if (eligible.Count == 0)
                return new List<Perk>();

            // 2. Shuffle eligible perks (Fisher-Yates)
            Shuffle(eligible, rng);

            // 3. Take top N
            return eligible.Take(count).ToList();
        }

        private static bool IsEligible(Perk perk, SelectionContext context)
        {
            // Simple eligibility checks
            if (context.UnlockedPerkIds.Contains(perk.Id)) return false; // Already have it (assuming perks are 1-time purchase)
            if (perk.MinLevel > context.PlayerLevel) return false;

            // Check prerequisites
            foreach (var prereqId in perk.PrerequisitePerkIds)
            {
                if (!context.UnlockedPerkIds.Contains(prereqId)) return false;
            }

            return true;
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
