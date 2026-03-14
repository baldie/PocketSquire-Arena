using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Attributes
    {
        public int Strength;
        public int Constitution;
        public int Magic;
        public int Dexterity;
        public int Luck;
        public int Defense;

        public static Attributes GetDefaultAttributes()
        {
            return new Attributes
            {
                Strength = 5,
                Constitution = 5,
                Magic = 5,
                Dexterity = 5,
                Luck = 5,
                Defense = 5
            };
        }

        public Attributes Clone()
        {
            return new Attributes
            {
                Strength = Strength,
                Constitution = Constitution,
                Magic = Magic,
                Dexterity = Dexterity,
                Luck = Luck,
                Defense = Defense
            };
        }
    }
}
