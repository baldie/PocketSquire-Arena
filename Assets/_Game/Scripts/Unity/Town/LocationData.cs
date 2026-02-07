using UnityEngine;
using System.Collections.Generic;
using PocketSquire.Arena.Core.Town;

namespace PocketSquire.Arena.Unity.Town
{
    /// <summary>
    /// ScriptableObject defining a town location's visual novel-style interior.
    /// Contains all data needed to populate the interior panel UI.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLocation", menuName = "PocketSquire/Town/LocationData")]
    public class LocationData : ScriptableObject
    {
        [Header("Location Info")]
        [SerializeField] private string locationName;

        [Header("Visuals")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite npcPortrait;

        [Header("Dialogue")]
        [TextArea(2, 5)]
        [SerializeField] private string initialGreeting;
        [SerializeField] private List<DialogueOption> dialogueOptions = new List<DialogueOption>();

        // Public accessors
        public string LocationName => locationName;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite NpcPortrait => npcPortrait;
        public string InitialGreeting => initialGreeting;
        public IReadOnlyList<DialogueOption> DialogueOptions => dialogueOptions;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(locationName))
            {
                locationName = name;
            }
        }
    }
}
