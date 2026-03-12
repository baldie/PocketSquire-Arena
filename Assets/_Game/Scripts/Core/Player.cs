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
            set => AcquiredPerks = value?.Select(id => GameWorld.GetPerkById(id)).Where(p => p != null).ToList() ?? new List<Perk>();
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
            set => ActivePerks = value?.Select(id => GameWorld.GetPerkById(id)).Where(p => p != null).ToList() ?? new List<Perk>();
        }

        [JsonIgnore]
        public Dictionary<string, PerkState> PerkStates { get; set; } = new();

        [JsonIgnore]
        public int MaxPerkSlots => PlayerClass.GetMaxPerkSlots(Class);

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

        public bool CanActivatePerk(string perkToRemove, Perk perkToActivate)
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
                        if (!AcquiredPerks.Any(p => p.Id == requiredPerkId)) return false;
                    }
                }
            }

            var futurePerks = new List<Perk>(ActivePerks);
            if (!string.IsNullOrEmpty(perkToRemove)) 
            {
                futurePerks.RemoveAll(p => p.Id == perkToRemove);
            }
            futurePerks.Add(perkToActivate);

            // 2. Check if there are multiple active satchel perks
            int activeSatchels = futurePerks.Count(p => p.Id == "satchel_tier_1" || p.Id == "satchel_tier_2" || p.Id == "satchel_tier_3");
            if (activeSatchels > 1) return false;

            // 3. Ensure we don't drop MaxSlots below our currently filled inventory
            int futureMaxSlots = Inventory.CalculateCapacity(futurePerks);
            
            return Inventory.Slots.Count <= futureMaxSlots;
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
            if (ActivePerks.Any(p => p.Id == perkId)) return false;
            if (ActivePerks.Count >= MaxPerkSlots) return false;
            ActivePerks.Add(perk);
            PerkStates[perkId] = new PerkState { PerkId = perkId };
            Inventory.UpdateCapacity(ActivePerks);
            return true;
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
