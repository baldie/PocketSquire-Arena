using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Player : Entity
    {
        public enum CharGender {
            m,
            f
        }

        public CharGender Gender;
        public int Level { get; private set; } = 1;
        public PlayerClass.ClassName Class { get; private set; } = PlayerClass.ClassName.Squire;
        public System.Collections.Generic.HashSet<string> UnlockedPerks { get; set; } = new System.Collections.Generic.HashSet<string>();

        public override string SpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_battle";
            }
        }

        public override string HitSpriteId
        {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_hit";
            }
        }

        public override string DefeatSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_defeat";
            }
        }

        public override string YieldSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_yield";
            }
        }

        public override string AttackSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_attack";
            }
        }

        public override string DefendSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_defend";
            }
        }

        public string ItemSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_item";
            }
        }

        public override string WinSpriteId {
            get
            {
                return "player_" + Gender.ToString() + "_l" + Level.ToString() + "_win";
            }
        }

        public bool CanLevelUp() {
            if (GameWorld.Progression == null) return false;
            var nextLevel = GameWorld.Progression.GetLevelForExperience(this.Experience);

            return this.Level < nextLevel;
        }

        public Player() : base() { }

        public Player(string name, int health, int maxHealth, Attributes attributes, CharGender gender) : base(name, health, maxHealth, attributes)
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

        public string GetSpriteId(GameContext context)
        {
            if (context == GameContext.Battle)
            {
                return this.SpriteId;
            }

            var sprite = "player_";
            sprite += this.Gender.ToString() + "_";
            sprite += this.Class.ToString().ToLower();
            switch(context)
            {
                case GameContext.Town: 
                    sprite += "_town";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            };
            return sprite;
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