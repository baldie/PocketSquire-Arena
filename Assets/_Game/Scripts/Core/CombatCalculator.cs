using System;

namespace PocketSquire.Arena.Core
{
    public static class CombatCalculator
    {
        public const int HpPerCon = 4;
        public const float DefDiminishingReturnsK = 2.5f;
        public const int BaseHitChance = 65;
        public const float ActorDexHitWeight = 1.5f;
        public const float ActorMagHitWeight = 1.0f;
        public const float ActorLckHitWeight = 0.3f;
        public const float ActorLevelHitWeight = 0.4f;
        public const float TargetDexDodgeWeight = 1.2f;
        public const float TargetLckDodgeWeight = 0.4f;
        public const int BaseCritChance = 3;
        public const float ActorLckCritWeight = 0.8f;
        public const float ActorLevelCritWeight = 0.2f;
        public const float RangedDexCritWeight = 0.3f;
        public const float MagicMagCritWeight = 0.2f;
        public const float PhysicalStrCritWeight = 0.15f;
        public const float SpecialAttackDamageBonus = 1.25f;
        public const int SpecialAttackHitPenalty = 10;
        public const float DefendBaseReduction = 0.50f;
        public const float DefendDefBonusPerPoint = 0.008f;
        public const float DefendMaxReduction = 0.80f;

        public readonly struct AttackResult
        {
            public AttackResult(
                PlayerClass.AttackStyle style,
                bool didHit,
                bool isCrit,
                int rawDamage,
                int damage,
                int finalDamage,
                int hitChance,
                int critChance)
            {
                Style = style;
                DidHit = didHit;
                IsCrit = isCrit;
                RawDamage = rawDamage;
                Damage = damage;
                FinalDamage = finalDamage;
                HitChance = hitChance;
                CritChance = critChance;
            }

            public PlayerClass.AttackStyle Style { get; }
            public bool DidHit { get; }
            public bool IsCrit { get; }
            public int RawDamage { get; }
            public int Damage { get; }
            public int FinalDamage { get; }
            public int HitChance { get; }
            public int CritChance { get; }
        }

        public static PlayerClass.AttackStyle GetAttackStyle(Entity actor)
        {
            if (actor is Player player)
            {
                return PlayerClass.GetAttackStyle(player.Class);
            }

            if (actor is Monster monster)
            {
                return monster.AttackStyle;
            }

            return PlayerClass.AttackStyle.Physical;
        }

        public static int CalculateBaseDamage(Entity actor, PlayerClass.AttackStyle style)
        {
            var actorAttributes = CombatUtilities.GetEffectiveAttributes(actor);
            int strength = actorAttributes.Strength;
            int dexterity = actorAttributes.Dexterity;
            int magic = actorAttributes.Magic;
            int luck = actorAttributes.Luck;

            int damage = style switch
            {
                PlayerClass.AttackStyle.Physical => strength + (int)Math.Floor(dexterity * 0.2f),
                PlayerClass.AttackStyle.Ranged => dexterity + (int)Math.Floor(strength * 0.15f),
                PlayerClass.AttackStyle.Magic => magic + (int)Math.Floor(luck * 0.1f),
                PlayerClass.AttackStyle.HybridPhysRanged => (int)Math.Floor(strength * 0.55f) + (int)Math.Floor(dexterity * 0.55f),
                PlayerClass.AttackStyle.HybridMagicRanged => (int)Math.Floor(magic * 0.65f) + (int)Math.Floor(dexterity * 0.4f),
                PlayerClass.AttackStyle.HybridPhysMagic => (int)Math.Floor(strength * 0.5f) + (int)Math.Floor(magic * 0.5f),
                _ => strength
            };

            return Math.Max(1, damage);
        }

        public static int CalculateHitChance(Entity actor, Entity target, PlayerClass.AttackStyle style, int actorLevel = 1)
        {
            var actorAttributes = CombatUtilities.GetEffectiveAttributes(actor);
            var targetAttributes = CombatUtilities.GetEffectiveAttributes(target);

            float actorAimStat = style == PlayerClass.AttackStyle.Magic
                ? actorAttributes.Magic * ActorMagHitWeight
                : actorAttributes.Dexterity * ActorDexHitWeight;

            float hitChance =
                BaseHitChance +
                actorAimStat +
                (actorLevel * ActorLevelHitWeight) +
                (actorAttributes.Luck * ActorLckHitWeight) -
                (targetAttributes.Dexterity * TargetDexDodgeWeight) -
                (targetAttributes.Luck * TargetLckDodgeWeight);

            return Math.Clamp((int)Math.Floor(hitChance), 5, 97);
        }

        public static int CalculateCritChance(Entity actor, PlayerClass.AttackStyle style, int actorLevel = 1)
        {
            var actorAttributes = CombatUtilities.GetEffectiveAttributes(actor);

            float critChance =
                BaseCritChance +
                (actorAttributes.Luck * ActorLckCritWeight) +
                (actorLevel * ActorLevelCritWeight);

            critChance += style switch
            {
                PlayerClass.AttackStyle.Ranged => actorAttributes.Dexterity * RangedDexCritWeight,
                PlayerClass.AttackStyle.Magic => actorAttributes.Magic * MagicMagCritWeight,
                PlayerClass.AttackStyle.Physical => actorAttributes.Strength * PhysicalStrCritWeight,
                PlayerClass.AttackStyle.HybridPhysRanged => ((actorAttributes.Strength * PhysicalStrCritWeight) + (actorAttributes.Dexterity * RangedDexCritWeight)) / 2f,
                PlayerClass.AttackStyle.HybridMagicRanged => ((actorAttributes.Magic * MagicMagCritWeight) + (actorAttributes.Dexterity * RangedDexCritWeight)) / 2f,
                PlayerClass.AttackStyle.HybridPhysMagic => ((actorAttributes.Strength * PhysicalStrCritWeight) + (actorAttributes.Magic * MagicMagCritWeight)) / 2f,
                _ => 0f
            };

            return Math.Clamp((int)Math.Floor(critChance), 1, 45);
        }

        public static float CalculateCritMultiplier(PlayerClass.AttackStyle style)
        {
            return style switch
            {
                PlayerClass.AttackStyle.Physical => 1.75f,
                PlayerClass.AttackStyle.Ranged => 2.0f,
                PlayerClass.AttackStyle.Magic => 1.5f,
                PlayerClass.AttackStyle.HybridPhysRanged => (1.75f + 2.0f) / 2f,
                PlayerClass.AttackStyle.HybridMagicRanged => (1.5f + 2.0f) / 2f,
                PlayerClass.AttackStyle.HybridPhysMagic => (1.75f + 1.5f) / 2f,
                _ => 1.75f
            };
        }

        public static int ApplyDefenseReduction(int rawDamage, Entity target)
        {
            if (rawDamage <= 0)
            {
                return 0;
            }

            var targetAttributes = CombatUtilities.GetEffectiveAttributes(target);
            float defenseMultiplier = 100f / (100f + (targetAttributes.Defense * DefDiminishingReturnsK));
            return Math.Max(1, (int)Math.Floor(rawDamage * defenseMultiplier));
        }

        public static float CalculateDefendDamageReduction(Entity defender)
        {
            var defenderAttributes = CombatUtilities.GetEffectiveAttributes(defender);
            float reductionPercent = DefendBaseReduction + (defenderAttributes.Defense * DefendDefBonusPerPoint);
            return Math.Clamp(reductionPercent, DefendBaseReduction, DefendMaxReduction);
        }

        public static int CalculateMaxHealth(int baseHp, int constitution)
        {
            return baseHp + (constitution * HpPerCon);
        }

        public static int GetClassBaseHP(PlayerClass.ClassName className)
        {
            return className switch
            {
                PlayerClass.ClassName.Squire => 18,

                PlayerClass.ClassName.Fighter => 22,
                PlayerClass.ClassName.Bowman => 18,
                PlayerClass.ClassName.SpellCaster => 14,

                PlayerClass.ClassName.Warrior => 26,
                PlayerClass.ClassName.Archer => 20,
                PlayerClass.ClassName.Mage => 16,
                PlayerClass.ClassName.Druid => 16,
                PlayerClass.ClassName.Hunter => 20,

                PlayerClass.ClassName.Knight => 30,
                PlayerClass.ClassName.Marksman => 22,
                PlayerClass.ClassName.Ranger => 22,
                PlayerClass.ClassName.Wizard => 18,
                PlayerClass.ClassName.Archdruid => 18,

                PlayerClass.ClassName.Sentinel => 34,
                PlayerClass.ClassName.Paladin => 34,
                PlayerClass.ClassName.Sniper => 24,
                PlayerClass.ClassName.Warden => 24,
                PlayerClass.ClassName.Sorcerer => 20,

                _ => 18
            };
        }

        public static AttackResult ResolveAttack(
            Entity actor,
            Entity target,
            bool isSpecial,
            Random rng,
            int actorLevel = 1,
            int hitChanceBonusPercent = 0,
            int critChanceBonusPercent = 0,
            float damageBuffMultiplier = 1f,
            float targetDamageReductionMultiplier = 1f,
            bool guaranteedHit = false,
            bool shouldDoubleDamage = false,
            int bonusDamageFlat = 0)
        {
            var style = GetAttackStyle(actor);
            int rawDamage = CalculateBaseDamage(actor, style);

            if (isSpecial)
            {
                rawDamage = Math.Max(1, (int)Math.Floor(rawDamage * SpecialAttackDamageBonus));
            }

            int hitChance = CalculateHitChance(actor, target, style, actorLevel) + hitChanceBonusPercent;
            if (isSpecial)
            {
                hitChance -= SpecialAttackHitPenalty;
            }
            hitChance = Math.Clamp(hitChance, 5, 97);

            bool didHit = guaranteedHit || rng.Next(100) < hitChance;

            int critChance = Math.Clamp(CalculateCritChance(actor, style, actorLevel) + critChanceBonusPercent, 1, 45);
            bool isCrit = didHit && rng.Next(100) < critChance;

            if (!didHit)
            {
                return new AttackResult(style, false, false, rawDamage, 0, 0, hitChance, critChance);
            }

            float critMultiplier = CalculateCritMultiplier(style);
            if (isSpecial)
            {
                critMultiplier *= SpecialAttackDamageBonus;
            }

            int damage = isCrit
                ? (int)Math.Floor(rawDamage * critMultiplier)
                : rawDamage;

            damage = Math.Max(1, (int)Math.Floor(damage * damageBuffMultiplier));

            if (shouldDoubleDamage)
            {
                damage *= 2;
            }

            damage += bonusDamageFlat;
            damage = Math.Max(1, damage);

            int finalDamage = ApplyDefenseReduction(damage, target);
            if (targetDamageReductionMultiplier < 1f)
            {
                finalDamage = Math.Max(1, (int)Math.Floor(finalDamage * targetDamageReductionMultiplier));
            }

            return new AttackResult(style, true, isCrit, rawDamage, damage, finalDamage, hitChance, critChance);
        }
    }
}
