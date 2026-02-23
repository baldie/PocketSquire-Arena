using System;
using Newtonsoft.Json;

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

        public System.Collections.Generic.HashSet<string> UnlockedPerks { get; set; } = new System.Collections.Generic.HashSet<string>();

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
    }
}