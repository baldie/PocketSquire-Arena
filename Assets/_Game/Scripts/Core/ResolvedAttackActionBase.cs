#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Shared attack action flow for regular and special attacks.
    /// The constructor resolves RNG immediately so queued actions remain immutable once created.
    /// </summary>
    public abstract class ResolvedAttackActionBase : IGameAction
    {
        protected ResolvedAttackActionBase(Entity actor, Entity target, bool isSpecial, Random? rng = null)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target ?? throw new ArgumentNullException(nameof(target));

            rng ??= new Random();

            var actorPassiveModifiers = actor is Player actorPlayer
                ? PerkProcessor.GetPassiveModifiers(actorPlayer)
                : new PerkProcessResult();

            var triggeredAttackModifiers = GetAttackStartModifiers(actor, target, rng);

            var targetPassiveModifiers = target is Player targetPlayer
                ? PerkProcessor.GetPassiveModifiers(targetPlayer)
                : new PerkProcessResult();

            int actorLevel = actor is Player player ? player.Level : 0;
            float actorDamageMultiplier =
                actorPassiveModifiers.DamageBuffMultiplier *
                triggeredAttackModifiers.DamageBuffMultiplier *
                triggeredAttackModifiers.DamageMultiplier;

            var resolution = CombatCalculator.ResolveAttack(
                actor,
                target,
                isSpecial,
                rng,
                actorLevel,
                actorPassiveModifiers.HitChanceBonusPercent + triggeredAttackModifiers.HitChanceBonusPercent,
                actorPassiveModifiers.CritChanceBonusPercent + triggeredAttackModifiers.CritChanceBonusPercent,
                actorDamageMultiplier,
                targetPassiveModifiers.DamageReductionMultiplier,
                triggeredAttackModifiers.GuaranteedHit,
                triggeredAttackModifiers.ShouldDoubleDamage,
                triggeredAttackModifiers.BonusDamageFlat);

            Style = resolution.Style;
            DidHit = resolution.DidHit;
            IsCrit = resolution.IsCrit;
            RawDamage = resolution.RawDamage;
            Damage = resolution.Damage;
            FinalDamage = resolution.FinalDamage;
        }

        public abstract ActionType Type { get; }
        protected abstract string MissText { get; }
        protected abstract string HitText { get; }
        protected abstract bool IsSpecialAttack { get; }

        protected PlayerClass.AttackStyle Style { get; }
        protected int RawDamage { get; }

        public Entity Actor { get; }
        public Entity Target { get; }
        public int Damage { get; }
        public int FinalDamage { get; protected set; }
        public bool DidHit { get; }
        public bool IsCrit { get; }

        public virtual void ApplyEffect()
        {
            if (!DidHit)
            {
                Console.WriteLine(MissText);
                HandleMissEvents();
                return;
            }

            int damageToApply = FinalDamage;
            if (Actor is Monster && Target is Player defendingPlayer)
            {
                damageToApply = ApplyIncomingHitPlayerPerks(defendingPlayer, damageToApply);
            }

            FinalDamage = damageToApply;
            string critNote = IsCrit ? " (CRIT!)" : string.Empty;
            Console.WriteLine($"{HitText}{FinalDamage} damage!{critNote}");

            if (Target is Player targetPlayer)
            {
                if (FinalDamage > 0)
                {
                    Target.TakeDamage(FinalDamage, dmg =>
                    {
                        var ctx = BuildPerkContext(targetPlayer, Actor, dmg, true, IsCrit);
                        var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.WouldDie, targetPlayer, ctx);
                        return result.SurviveFatalBlow;
                    });
                }
            }
            else
            {
                Target.TakeDamage(FinalDamage);
            }

            HandleHitEvents();
        }

        private PerkProcessResult GetAttackStartModifiers(Entity actor, Entity target, Random rng)
        {
            if (actor is Player player && target is Monster)
            {
                int previewDamage = CombatCalculator.CalculateBaseDamage(actor, CombatCalculator.GetAttackStyle(actor));
                var ctx = BuildPerkContext(player, target, previewDamage, false, false, rng);
                return PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerAttackedMonster, player, ctx);
            }

            return new PerkProcessResult();
        }

        private void HandleMissEvents()
        {
            if (Actor is Player playerActor && Target is Monster)
            {
                var ctx = BuildPerkContext(playerActor, Target, 0, false, false);
                PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerMissedMonster, playerActor, ctx);
                if (IsSpecialAttack)
                {
                    PerkProcessor.ProcessEvent(PerkTriggerEvent.SpecialAttackMissed, playerActor, ctx);
                }
                return;
            }

            if (Actor is Monster && Target is Player playerTarget)
            {
                var ctx = BuildPerkContext(playerTarget, Actor, 0, false, false);
                PerkProcessor.ProcessEvent(PerkTriggerEvent.MonsterMissedPlayer, playerTarget, ctx);
            }
        }

        private void HandleHitEvents()
        {
            if (Actor is Player playerActor && Target is Monster)
            {
                var ctx = BuildPerkContext(playerActor, Target, FinalDamage, true, IsCrit);
                PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerHitMonster, playerActor, ctx);
                if (IsSpecialAttack)
                {
                    PerkProcessor.ProcessEvent(PerkTriggerEvent.SpecialAttackLanded, playerActor, ctx);
                }
            }
        }

        private int ApplyIncomingHitPlayerPerks(Player targetPlayer, int damage)
        {
            var ctx = BuildPerkContext(targetPlayer, Actor, damage, true, IsCrit);
            var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.MonsterAttackHitPlayer, targetPlayer, ctx);

            if (result.NullifyDamage)
            {
                return 0;
            }

            if (result.DamageReductionMultiplier < 1f && damage > 0)
            {
                return Math.Max(1, (int)Math.Floor(damage * result.DamageReductionMultiplier));
            }

            return damage;
        }

        private static PerkContext BuildPerkContext(Player player, Entity target, int damage, bool didHit, bool isCrit, Random? rng = null)
        {
            return new PerkContext
            {
                Player = player,
                Target = target,
                Damage = damage,
                DidHit = didHit,
                IsCrit = isCrit,
                PlayerHpPercent = player.MaxHealth > 0 ? (int)(player.Health * 100f / player.MaxHealth) : 0,
                TargetHpPercent = target.MaxHealth > 0 ? (int)(target.Health * 100f / target.MaxHealth) : 0,
                Rng = rng ?? new Random()
            };
        }
    }
}
