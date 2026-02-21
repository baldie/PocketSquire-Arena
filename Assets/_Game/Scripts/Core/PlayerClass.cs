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
                case ClassName.Squire: return "A young knight in training, balanced in attack and defense.";
                case ClassName.SpellCaster: return "A novice magic user, focusing on basic elemental spells.";
                case ClassName.Bowman: return "An entry-level ranged attacker wielding a basic bow.";
                case ClassName.Fighter: return "A melee combatant prioritizing pure physical strength.";
                case ClassName.Mage: return "An intermediate spellcaster with access to diverse magical abilities.";
                case ClassName.Druid: return "A nature-wielding adept who can harness the earth's power.";
                case ClassName.Archer: return "A seasoned combatant skilled in striking from a distance.";
                case ClassName.Hunter: return "A survivalist who excels at tracking and trapping foes.";
                case ClassName.Warrior: return "A heavily armed frontline combatant built for endurance.";
                case ClassName.Wizard: return "A learned scholar of the arcane, mastering complex spells.";
                case ClassName.Archdruid: return "A master of nature manipulation and elemental synthesis.";
                case ClassName.Marksman: return "A precise ranged combatant capable of devastating long-shots.";
                case ClassName.Ranger: return "A versatile survivor blending ranged combat with woodland magic.";
                case ClassName.Knight: return "A noble warrior encased in heavy armor, protecting the weak.";
                case ClassName.Sorcerer: return "A wielder of innate magical potential, unleashing raw arcane power.";
                case ClassName.Warden: return "A steadfast guardian trained to protect nature and allies alike.";
                case ClassName.Sniper: return "A specialist in high-damage, single-target ranged strikes.";
                case ClassName.Sentinel: return "An impenetrable frontline defender with unmatched resilience.";
                case ClassName.Paladin: return "A holy crusader combining divine magic with martial prowess.";
                default: return "An unknown hero.";
            }
        }
    }
}
