using UnityEngine;
using System.Collections.Generic;
using PocketSquire.Arena.Core.Town;
using PocketSquire.Arena.Core.Perks;
using PocketSquire.Arena.Unity.LevelUp;

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

        [Header("Audio")]
        [SerializeField] private AudioClip entrySound;

        [Header("Dialogue")]
        [TextArea(2, 5)]
        [SerializeField] private string initialGreeting;
        [SerializeField] private List<DialogueOption> dialogueOptions = new List<DialogueOption>();

        [Header("Shop")]
        [HideInInspector] [SerializeField] private List<int> shopItemIds = new List<int>();

        [Header("Arena Perks")]
        [SerializeField] private bool hasVendorType;
        [SerializeField] private VendorType vendorType;

        // Public accessors
        public string LocationName => locationName;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite NpcPortrait => npcPortrait;
        public AudioClip EntrySound => entrySound;
        public string InitialGreeting => initialGreeting;
        public IReadOnlyList<DialogueOption> DialogueOptions => dialogueOptions;
        public IReadOnlyList<int> ShopItemIds => shopItemIds;

        /// <summary>
        /// The vendor type for arena perks. Null if this location doesn't sell arena perks.
        /// Assign in the Unity Editor.
        /// </summary>
        public VendorType? VendorType => hasVendorType ? vendorType : (VendorType?)null;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(locationName))
            {
                locationName = name;
            }
        }
    }
}

