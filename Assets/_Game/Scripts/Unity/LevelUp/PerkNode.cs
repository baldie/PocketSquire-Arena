using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core.LevelUp;
using static PocketSquire.Arena.Core.Player;

namespace PocketSquire.Arena.Unity.LevelUp
{
    [CreateAssetMenu(fileName = "NewPerkNode", menuName = "PocketSquire/LevelUp/PerkNode")]
    public class PerkNode : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private int minLevel;
        [SerializeField] private List<PerkNode> prerequisites = new List<PerkNode>();
        public List<PlayerClass> allowedClasses = new List<PlayerClass>();

        public string Id => id;

        public Perk ToCorePerk()
        {
            var prereqIds = prerequisites.Where(p => p != null).Select(p => p.Id).ToList();
            // Pass the allowedClasses through to the core logic
            return new Perk(id, displayName, description, minLevel, prereqIds, new List<PlayerClass>(allowedClasses));
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
