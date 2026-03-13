using System;

namespace PocketSquire.Arena.Core
{
    public class PlayerClass
    {
        public readonly struct ManaProfile
        {
            public ManaProfile(bool usesMana, int baseManaCost, int regenPerTurn)
            {
                UsesMana = usesMana;
                BaseManaCost = baseManaCost;
                RegenPerTurn = regenPerTurn;
            }

            public bool UsesMana { get; }
            public int BaseManaCost { get; }
            public int RegenPerTurn { get; }
        }

        public enum AttackStyle
        {
            Physical,
            Ranged,
            Magic,
            HybridPhysRanged,
            HybridMagicRanged,
            HybridPhysMagic
        }

        public enum ClassName
        {
            // Tier 0
            Squire,
            // Tier 1
            SpellCaster,
            Bowman,
            Fighter,
            // Tier 2
            Mage,
            Druid,
            Archer,
            Hunter,
            Warrior,
            // Tier 3
            Wizard,
            Archdruid,
            Marksman,
            Ranger,
            Knight,
            // Prestige Classes
            Sorcerer,
            Warden,
            Sniper,
            Sentinel,
            Paladin
        }

        /// <summary>Returns the tier (0–4) for a class. Prestige classes are tier 4.</summary>
        public static int GetTier(ClassName className)
        {
            switch (className)
            {
                case ClassName.Squire: return 0;
                case ClassName.SpellCaster:
                case ClassName.Bowman:
                case ClassName.Fighter: return 1;
                case ClassName.Mage:
                case ClassName.Druid:
                case ClassName.Archer:
                case ClassName.Hunter:
                case ClassName.Warrior: return 2;
                case ClassName.Wizard:
                case ClassName.Archdruid:
                case ClassName.Marksman:
                case ClassName.Ranger:
                case ClassName.Knight: return 3;
                default: return 4; // Prestige classes
            }
        }

        /// <summary>
        /// Tier 0→2 slots, Tier 1→4, Tier 2→6, Tier 3→8, Prestige→10.
        /// </summary>
        public static int GetMaxPerkSlots(ClassName className)
        {
            return (GetTier(className) + 1) * 2;
        }

        public static string GetDescription(ClassName className)
        {
            switch (className)
            {
                case ClassName.Squire: return "A young adventurer in training, balanced in attack and defense.";
                case ClassName.SpellCaster: return "A novice magic user, channeling spells from the grimoire.";
                case ClassName.Bowman: return "An entry-level ranged attacker wielding a basic bow.";
                case ClassName.Fighter: return "A novice melee combatant with sword and shield.";
                case ClassName.Mage: return "An intermediate spellcaster able to channel energy from mana.";
                case ClassName.Druid: return "A nature-wielding adept who can harness the earth's power.";
                case ClassName.Archer: return "A seasoned combatant skilled in striking from a distance.";
                case ClassName.Hunter: return "A survivalist who excels with thrown knife attacks.";
                case ClassName.Warrior: return "An armed frontline combatant built for endurance.";
                case ClassName.Wizard: return "A learned scholar of the arcane, mastering complex spells.";
                case ClassName.Archdruid: return "A master of nature manipulation and elemental synthesis.";
                case ClassName.Marksman: return "A precise crossbow combatant capable of devastating long-shots.";
                case ClassName.Ranger: return "A versatile survivor blending ranged combat with melee prowess.";
                case ClassName.Knight: return "A noble warrior able to wear heavy armor with both sword and shield.";
                case ClassName.Sorcerer: return "One who has traded his soul for limitless arcane power.";
                case ClassName.Warden: return "One who has merged souls with nature to gain beast-like powers.";
                case ClassName.Sniper: return "One who drifts the plains alone able to bring down foes with precision.";
                case ClassName.Sentinel: return "One who has given his soul over for raw physical power.";
                case ClassName.Paladin: return "One who has forsaken his earthly soul for divine powers.";
                default: return "An unknown hero.";
            }
        }

        public static AttackStyle GetAttackStyle(ClassName className)
        {
            switch (className)
            {
                case ClassName.Bowman:
                case ClassName.Archer:
                case ClassName.Marksman:
                case ClassName.Sniper:
                    return AttackStyle.Ranged;

                case ClassName.SpellCaster:
                case ClassName.Mage:
                case ClassName.Wizard:
                case ClassName.Sorcerer:
                    return AttackStyle.Magic;

                case ClassName.Hunter:
                case ClassName.Ranger:
                    return AttackStyle.HybridPhysRanged;

                case ClassName.Druid:
                case ClassName.Archdruid:
                case ClassName.Warden:
                    return AttackStyle.HybridMagicRanged;

                case ClassName.Paladin:
                    return AttackStyle.HybridPhysMagic;

                case ClassName.Squire:
                case ClassName.Fighter:
                case ClassName.Warrior:
                case ClassName.Knight:
                case ClassName.Sentinel:
                default:
                    return AttackStyle.Physical;
            }
        }

        public static ManaProfile GetManaProfile(ClassName className)
        {
            return className switch
            {
                ClassName.SpellCaster => new ManaProfile(true, 8, 4),
                ClassName.Mage => new ManaProfile(true, 12, 5),
                ClassName.Druid => new ManaProfile(true, 10, 4),
                ClassName.Wizard => new ManaProfile(true, 16, 6),
                ClassName.Archdruid => new ManaProfile(true, 14, 5),
                ClassName.Sorcerer => new ManaProfile(true, 20, 7),
                ClassName.Warden => new ManaProfile(true, 12, 5),
                ClassName.Paladin => new ManaProfile(true, 10, 4),
                _ => new ManaProfile(false, 0, 0)
            };
        }
    }
}
