using System.Collections.Generic;
using UnityEngine;
using PocketSquire.Arena.Core;


[CreateAssetMenu(fileName = "EvolutionTree", menuName = "PocketSquire/Evolution Tree")]
public class EvolutionTreeSO : ScriptableObject
{
    [System.Serializable]
    public struct EvolutionNode
    {
        public Player.PlayerClass currentClass;
        public int unlockLevel;
        public List<Player.PlayerClass> potentialEvolutions;
    }

    public List<EvolutionNode> nodes;

    // Helper to quickly find what a class can become at a specific level
    public List<Player.PlayerClass> GetAvailableEvolutions(Player.PlayerClass currentClass, int level)
    {
        var node = nodes.Find(n => n.currentClass == currentClass);
        
        // Return evolutions only if level threshold is met
        if (node.potentialEvolutions != null && level >= node.unlockLevel)
        {
            return node.potentialEvolutions;
        }

        return new List<Player.PlayerClass>(); // Empty list if no evolution available
    }
}