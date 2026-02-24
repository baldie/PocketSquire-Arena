using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Unity.LevelUp
{
    [CreateAssetMenu(fileName = "NewPerkNode", menuName = "PocketSquire/LevelUp/PerkNode")]
    public class PerkNode : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private int minLevel;
        [SerializeField] private int price;
        [SerializeField] private PerkEffectType effectType = PerkEffectType.None;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<PerkNode> prerequisites = new List<PerkNode>();
        public List<PlayerClass.ClassName> allowedClasses = new List<PlayerClass.ClassName>();

        public string Id => id;
        public int Price => price;
        public Sprite Icon => icon;

        public Perk ToCorePerk()
        {
            var prereqIds = prerequisites.Where(p => p != null).Select(p => p.Id).ToList();
            // Pass allowedClasses, price, and effectType through to the core logic
            return new Perk(id, displayName, description, minLevel, prereqIds, new List<PlayerClass.ClassName>(allowedClasses), price, effectType);
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
