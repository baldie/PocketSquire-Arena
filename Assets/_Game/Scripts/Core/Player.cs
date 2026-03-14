using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public enum Genders {
            m,
            f
        }

        public Genders Gender { get; set; }
        public int Level { get; private set; } = 1;

        [JsonProperty("class")]
        public PlayerClass.ClassName Class { get; private set; } = PlayerClass.ClassName.Squire;

        [JsonIgnore]
        public List<Perk> AcquiredPerks { get; set; } = new List<Perk>();

        [JsonProperty("AcquiredPerks")]
        public string[] SerializedAcquiredPerks
        {
            get => AcquiredPerks.Select(p => p.Id).ToArray();
            set => AcquiredPerks = value?.Select(id => GameWorld.GetPerkById(id)).OfType<Perk>().ToList() ?? new List<Perk>();
        }

        public System.Collections.Generic.HashSet<string> UnlockedClasses { get; set; } = new System.Collections.Generic.HashSet<string> { PlayerClass.ClassName.Squire.ToString() };

        // Arena Perk state — AcquiredPerks and ActivePerks are serialised.
        // PerkStates is runtime-only and rebuilt by InitializePerkStates() after load.
        [JsonIgnore]
        public List<Perk> ActivePerks { get; set; } = new List<Perk>();

        [JsonProperty("ActivePerkIds")]
        public string[] SerializedActivePerks
        {
            get => ActivePerks.Select(p => p.Id).ToArray();
            set => ActivePerks = value?.Select(id => GameWorld.GetPerkById(id)).OfType<Perk>().ToList() ?? new List<Perk>();
        }

        [JsonIgnore]
        public Dictionary<string, PerkState> PerkStates { get; set; } = new();

        [JsonIgnore]
        public int MaxPerkSlots => PlayerClass.GetMaxPerkSlots(Class);

        [JsonIgnore]
        public bool UsesMana => PlayerClass.GetManaProfile(Class).UsesMana;

        [JsonIgnore]
        public int SpecialAttackManaCost => PlayerClass.GetManaProfile(Class).BaseManaCost;

        [JsonIgnore]
        public int ManaRegenPerTurn => PlayerClass.GetManaProfile(Class).RegenPerTurn;

        public override string SpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_battle";
            }
        }

        public override string HitSpriteId
        {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_hit";
            }
        }

        public override string DefeatSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_defeat";
            }
        }

        public override string BattleSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_battle";
            }
        }

        public override string AttackSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_attack";
            }
        }

        public override string DefendSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_defend";
            }
        }

        public string ItemSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_item";
            }
        }

        public override string WinSpriteId {
            get
            {
                return Gender.ToString() + "_" + Class.ToString().ToLower() + "_win";
            }
        }

        public bool CanLevelUp() {
            if (GameWorld.Progression == null) return false;
            var nextLevel = GameWorld.Progression.GetLevelForExperience(this.Experience);

            return this.Level < nextLevel;
        }

        public Player() : base() { }

        public Player(string name, int health, int maxHealth, Attributes attributes, Genders gender) : base(name, health, maxHealth, attributes)
        {
            this.Gender = gender;
        }

        public void GainExperience(int amount)
        {
            Experience += amount;
        }

        public void GainGold(int amount)
        {
            Gold += amount;
        }

        public void SpendGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            }
            if (Gold < amount)
            {
                throw new InvalidOperationException($"Insufficient gold. Have {Gold}, need {amount}");
            }
            Gold -= amount;
        }

        public bool TryPurchaseItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Gold < item.Price)
            {
                return false;
            }

            // Check inventory space before spending gold
            if (!Inventory.HasRoom(item.Id))
            {
                return false;
            }

            SpendGold(item.Price);
            Inventory.AddItem(item.Id, 1);
            return true;
        }


        public void AcceptNewLevel() {
            if (GameWorld.Progression != null) {
                this.Level = GameWorld.Progression.GetLevelForExperience(this.Experience);
            }
        }

        public void RecalculateMaxHealth()
        {
            MaxHealth = CombatCalculator.CalculateMaxHealth(
                CombatCalculator.GetClassBaseHP(Class),
                Attributes.Constitution);
        }

        public bool CanAffordSpecialAttack()
        {
            return !UsesMana || Mana >= SpecialAttackManaCost;
        }

        public bool TrySpendManaForSpecialAttack()
        {
            if (!UsesMana)
            {
                return true;
            }

            if (Mana < SpecialAttackManaCost)
            {
                return false;
            }

            Mana -= SpecialAttackManaCost;
            return true;
        }

        public void ChangeClass(PlayerClass.ClassName newClass)
        {
            this.Class = newClass;
            var template = GameWorld.GetClassTemplate(this.Gender, newClass);
            if (template != null)
            {
                this.Attributes = new Attributes
                {
                    Strength = template.Attributes.Strength,
                    Constitution = template.Attributes.Constitution,
                    Magic = template.Attributes.Magic,
                    Dexterity = template.Attributes.Dexterity,
                    Luck = template.Attributes.Luck,
                    Defense = template.Attributes.Defense
                };
                // Also update attack/defend/hit/defeat sound ids if applicable
                this.AttackSoundId = template.AttackSoundId;
                this.DefendSoundId = template.DefendSoundId;
                this.HitSoundId = template.HitSoundId;
                this.DefeatSoundId = template.DefeatSoundId;
                this.SpecialAttackSoundId = template.SpecialAttackSoundId;
                this.Mana = template.Mana;
                this.MaxMana = template.MaxMana;
                RecalculateMaxHealth();
                this.Health = this.MaxHealth;
                this.Mana = this.MaxMana;
                Console.WriteLine($"Player health reset to {this.Health}");
            }
        }

        public string GetSpriteId()
        {
            return this.SpriteId;
        }

        public override string GetActionSoundId(ActionType actionType)
        {
            switch(actionType)
            {
                case ActionType.Attack:
                case ActionType.SpecialAttack:
                    return !string.IsNullOrEmpty(AttackSoundId) ? AttackSoundId : "player_attack";
                case ActionType.Item:
                    return "player_item";
                case ActionType.Yield:
                    return string.Empty;
                default:
                    return string.Empty;
            };
        }

        public override string ToString()
        {
            return $"[Player: {Name} (Lvl {Level} {Class})] HP: {Health}/{MaxHealth}, Gold: {Gold}, Exp: {Experience}, Attr: [Str:{Attributes.Strength} Def:{Attributes.Defense} Lck:{Attributes.Luck}]";
        }

        // --- Arena Perk Methods ---

        public PerkActivationAttemptResult CanActivatePerk(PerkActivationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var perkToActivate = request.PerkToActivate;
            if (perkToActivate == null)
            {
                return PerkActivationAttemptResult.Failure("Perk could not be found.");
            }

            if (!AcquiredPerks.Any(p => p.Id == perkToActivate.Id))
            {
                return PerkActivationAttemptResult.Failure("You do not own this perk.");
            }

            if (ActivePerks.Any(p => p.Id == perkToActivate.Id && p.Id != request.PerkToRemoveId))
            {
                return PerkActivationAttemptResult.Failure("That perk is already active.");
            }

            // 1. Check prerequisites explicitly on the perk itself
            if (perkToActivate.Prerequisites != null)
            {
                if (Level < perkToActivate.Prerequisites.MinLevel)
                {
                    return PerkActivationAttemptResult.Failure($"Requires Lv{perkToActivate.Prerequisites.MinLevel}.");
                }

                if (!string.IsNullOrEmpty(perkToActivate.Prerequisites.ClassName))
                {
                    if (Class.ToString() != perkToActivate.Prerequisites.ClassName)
                    {
                        return PerkActivationAttemptResult.Failure($"Requires {perkToActivate.Prerequisites.ClassName}.");
                    }
                }

                if (perkToActivate.Prerequisites.RequiredPerks != null)
                {
                    var missingRequiredPerks = perkToActivate.Prerequisites.RequiredPerks
                        .Where(requiredPerkId => !AcquiredPerks.Any(p => p.Id == requiredPerkId))
                        .Select(GetPerkRequirementLabel)
                        .ToList();

                    if (missingRequiredPerks.Count > 0)
                    {
                        return PerkActivationAttemptResult.Failure($"Requires {string.Join(", ", missingRequiredPerks)}.");
                    }
                }
            }

            var futurePerks = new List<Perk>(ActivePerks);
            if (!string.IsNullOrEmpty(request.PerkToRemoveId))
            {
                futurePerks.RemoveAll(p => p.Id == request.PerkToRemoveId);
            }
            futurePerks.Add(perkToActivate);

            // 2. Check if there are multiple active satchel perks
            int activeSatchels = futurePerks.Count(p => IsSatchelPerk(p.Id));
            if (activeSatchels > 1)
            {
                return PerkActivationAttemptResult.Failure("Only one satchel perk can be active.");
            }

            // 3. Ensure we don't drop MaxSlots below our currently filled inventory
            int futureMaxSlots = Inventory.CalculateCapacity(futurePerks);
            if (Inventory.Slots.Count > futureMaxSlots)
            {
                return PerkActivationAttemptResult.Failure("Not enough inventory space to activate this perk.");
            }

            return PerkActivationAttemptResult.Success();
        }

        public bool TryPurchasePerk(Perk perk)
        {
            if (perk == null) throw new ArgumentNullException(nameof(perk));
            if (AcquiredPerks.Any(p => p.Id == perk.Id)) return false;
            if (Gold < perk.Cost) return false;
            SpendGold(perk.Cost);
            AcquiredPerks.Add(perk);
            return true;
        }

        public bool TryActivatePerk(string perkId)
        {
            var perk = AcquiredPerks.FirstOrDefault(p => p.Id == perkId);
            if (perk == null) return false;
            if (ActivePerks.Count >= MaxPerkSlots) return false;
            var activationAttempt = CanActivatePerk(new PerkActivationRequest
            {
                PerkToActivate = perk
            });
            if (!activationAttempt.Succeeded) return false;
            ActivePerks.Add(perk);
            PerkStates[perkId] = new PerkState { PerkId = perkId };
            Inventory.UpdateCapacity(ActivePerks);
            return true;
        }

        private static bool IsSatchelPerk(string perkId)
        {
            return perkId == "satchel_tier_1" || perkId == "satchel_tier_2" || perkId == "satchel_tier_3";
        }

        private static string GetPerkRequirementLabel(string perkId)
        {
            var requiredPerk = GameWorld.GetPerkById(perkId);
            if (requiredPerk != null && !string.IsNullOrEmpty(requiredPerk.DisplayName))
            {
                return requiredPerk.DisplayName;
            }

            return perkId;
        }

        public bool TryDeactivatePerk(string perkId)
        {
            var perkToRemove = ActivePerks.FirstOrDefault(p => p.Id == perkId);
            if (perkToRemove == null || !ActivePerks.Remove(perkToRemove)) return false;
            PerkStates.Remove(perkId);
            Inventory.UpdateCapacity(ActivePerks);
            return true;
        }

        /// <summary>
        /// Rebuilds runtime PerkStates from the serialised ActivePerks list.
        /// Call after loading a save file.
        /// </summary>
        public void InitializePerkStates()
        {
            PerkStates.Clear();
            foreach (var perk in ActivePerks)
                PerkStates[perk.Id] = new PerkState { PerkId = perk.Id };
        }
    }
}
