using System;

namespace PocketSquire.Arena.Core
{
    public class PlayerClass
    {
        public enum ClassName
        {
            Squire,
            SpellCaster,
            Bowman,
            Fighter,
            Mage,
            Druid,
            Archer,
            Hunter,
            Warrior,
            Wizard,
            Archdruid,
            Marksman,
            Ranger,
            Knight,
            Sorcerer,
            Warden,
            Sniper,
            Sentinel,
            Paladin
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
    }
}
