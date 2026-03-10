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

        public System.Collections.Generic.HashSet<string> AcquiredPerks { get; set; } = new System.Collections.Generic.HashSet<string>();
        public System.Collections.Generic.HashSet<string> UnlockedClasses { get; set; } = new System.Collections.Generic.HashSet<string> { PlayerClass.ClassName.Squire.ToString() };

        // Arena Perk state — AcquiredPerks and ActiveArenaPerkIds are serialised.
        // ArenaPerkStates is runtime-only and rebuilt by InitializeArenaPerkStates() after load.
        public List<string> ActiveArenaPerkIds { get; set; } = new();

        public List<ArenaPerk> GetActivePerks()
        {
            return ActiveArenaPerkIds.Select(id => GameWorld.GetArenaPerkById(id)).ToList();
        }

        public List<ArenaPerk> GetAcquiredPerks()
        {
            return AcquiredPerks.Select(id => GameWorld.GetArenaPerkById(id)).ToList();
        }

        [JsonIgnore]
        public Dictionary<string, ArenaPerkState> ArenaPerkStates { get; set; } = new();

        [JsonIgnore]
        public int MaxArenaPerkSlots => PlayerClass.GetMaxPerkSlots(Class);

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

        public bool TryPurchasePerk(LevelUp.Perk perk)
        {
            if (perk == null)
            {
                throw new ArgumentNullException(nameof(perk));
            }

            // Perks are one-time purchases
            if (AcquiredPerks.Contains(perk.Id))
            {
                return false;
            }

            if (Gold < perk.Price)
            {
                return false;
            }

            SpendGold(perk.Price);
            AcquiredPerks.Add(perk.Id);

            // Dispatch effect based on the perk's configured EffectType
            ApplyPerkEffect(perk);

            return true;
        }

        private void ApplyPerkEffect(LevelUp.Perk perk)
        {
            switch (perk.EffectType)
            {
                case LevelUp.PerkEffectType.None:
                default:
                    // Tracking or un-implemented mechanic
                    break;
                    // Tracking or un-implemented mechanic
                    break;
            }
        }

        public void AcceptNewLevel() {
            if (GameWorld.Progression != null) {
                this.Level = GameWorld.Progression.GetLevelForExperience(this.Experience);
            }
        }

        public void ChangeClass(PlayerClass.ClassName newClass)
        {
            this.Class = newClass;
            var template = GameWorld.GetClassTemplate(this.Gender, newClass);
            if (template != null)
            {
                this.Health = this.MaxHealth;
                Console.WriteLine($"Player health reset to {this.Health}");
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

        public bool CanActivateArenaPerk(string perkToRemove, ArenaPerk perkToActivate)
        {
            if (perkToActivate == null) return false;

            // 1. Check prerequisites explicitly on the perk itself
            if (perkToActivate.Prerequisites != null)
            {
                if (Level < perkToActivate.Prerequisites.MinLevel) return false;
                
                if (!string.IsNullOrEmpty(perkToActivate.Prerequisites.ClassName))
                {
                    if (Class.ToString() != perkToActivate.Prerequisites.ClassName) return false;
                }

                if (perkToActivate.Prerequisites.RequiredPerks != null)
                {
                    foreach (var requiredPerkId in perkToActivate.Prerequisites.RequiredPerks)
                    {
                        if (!AcquiredPerks.Contains(requiredPerkId)) return false;
                    }
                }
            }

            var futurePerkIds = new List<string>(ActiveArenaPerkIds);
            if (!string.IsNullOrEmpty(perkToRemove)) futurePerkIds.Remove(perkToRemove);
            futurePerkIds.Add(perkToActivate.Id);

            // 2. Check if there are multiple active satchel perks
            int activeSatchels = futurePerkIds.Count(id => id == "satchel_tier_1" || id == "satchel_tier_2" || id == "satchel_tier_3");
            if (activeSatchels > 1) return false;

            // 3. Ensure we don't drop MaxSlots below our currently filled inventory
            var futurePerks = futurePerkIds.Select(id => GameWorld.GetArenaPerkById(id)).ToList();
            int futureMaxSlots = Inventory.CalculateCapacity(futurePerks);
            
            return Inventory.Slots.Count <= futureMaxSlots;
        }

        public bool TryPurchaseArenaPerk(ArenaPerk perk)
        {
            if (perk == null) throw new ArgumentNullException(nameof(perk));
            if (AcquiredPerks.Contains(perk.Id)) return false;
            if (Gold < perk.Cost) return false;
            SpendGold(perk.Cost);
            AcquiredPerks.Add(perk.Id);
            return true;
        }

        public bool TryActivateArenaPerk(string perkId)
        {
            if (!AcquiredPerks.Contains(perkId)) return false;
            if (ActiveArenaPerkIds.Contains(perkId)) return false;
            if (ActiveArenaPerkIds.Count >= MaxArenaPerkSlots) return false;
            ActiveArenaPerkIds.Add(perkId);
            ArenaPerkStates[perkId] = new ArenaPerkState { PerkId = perkId };
            Inventory.UpdateCapacity(GetActivePerks());
            return true;
        }

        public bool TryDeactivateArenaPerk(string perkId)
        {
            if (!ActiveArenaPerkIds.Remove(perkId)) return false;
            ArenaPerkStates.Remove(perkId);
            Inventory.UpdateCapacity(GetActivePerks());
            return true;
        }

        /// <summary>
        /// Rebuilds runtime ArenaPerkStates from the serialised ActiveArenaPerkIds list.
        /// Call after loading a save file.
        /// </summary>
        public void InitializeArenaPerkStates()
        {
            ArenaPerkStates.Clear();
            foreach (var id in ActiveArenaPerkIds)
                ArenaPerkStates[id] = new ArenaPerkState { PerkId = id };
        }
    }
}
