using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core.PowerUps;

/// <summary>
/// Component that configures the PowerUpIcon prefab with PowerUp data.
/// Attach to the root of the PowerUpIcon prefab.
/// </summary>
public class PowerUpIconController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Background image that displays rarity color")]
    public Image rarityBackground;
    
    [Tooltip("Main icon image for the power-up")]
    public Image powerUpIconImage;
    
    [Header("Rank Indicators")]
    [Tooltip("Rank I indicator (always shown for Rank I+)")]
    public GameObject rankI;
    
    [Tooltip("Rank II indicator (shown for Rank II+)")]
    public GameObject rankII;
    
    [Tooltip("Rank III indicator (shown for Rank III only)")]
    public GameObject rankIII;
    
    [Header("Indicators")]
    [Tooltip("Indicator icon shown for Monster Debuffs (Curses)")]
    public GameObject curseIndicator;

    [Header("Materials")]
    [Tooltip("Material to use for grayscale effect (for monster debuffs)")]
    public Material grayscaleMaterial;

    /// <summary>
    /// Configures this icon with the given PowerUp data.
    /// Sets rarity background color, icon sprite, rank visibility, and PowerUpSelector.
    /// </summary>
    public void Configure(PowerUp powerUp)
    {
        if (powerUp == null)
        {
            Debug.LogWarning("[PowerUpIconController] PowerUp is null, cannot configure icon");
            return;
        }

        bool isMonsterDebuff = powerUp.Component.ComponentType == PowerUpComponentType.MonsterDebuff;

        // Set rarity background color
        if (rarityBackground != null)
        {
            rarityBackground.color = GetRarityColor(powerUp.Component.Rarity);
        }
        else
        {
            Debug.LogWarning("[PowerUpIconController] RarityBackground is not assigned");
        }

        // Set icon sprite
        if (powerUpIconImage != null)
        {
            var sprite = GameAssetRegistry.Instance.GetSprite(powerUp.Component.IconId);
            if (sprite != null)
            {
                powerUpIconImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[PowerUpIconController] Icon sprite '{powerUp.Component.IconId}' not found for {powerUp.DisplayName}");
            }

            // Apply grey tint for monster debuffs
            if (isMonsterDebuff)
            {
                powerUpIconImage.material = grayscaleMaterial;
            }
            else
            {
                powerUpIconImage.material = null;
            }
        }
        else
        {
            Debug.LogWarning("[PowerUpIconController] PowerUpIconImage is not assigned");
        }
        
        // Show/Hide Curse Indicator
        if (curseIndicator != null)
        {
            curseIndicator.SetActive(isMonsterDebuff);
        }
        else
        {
            Debug.LogWarning("[PowerUpIconController] CurseIndicator is not assigned");
        }

        // Configure rank visibility
        ConfigureRankVisibility(powerUp.Rank);

        // Ensure PowerUpSelector is attached and initialized
        var selector = GetComponent<PowerUpSelector>();
        if (selector == null)
        {
            selector = gameObject.AddComponent<PowerUpSelector>();
        }
        selector.Initialize(powerUp);
    }

    /// <summary>
    /// Sets rank indicator visibility based on the power-up's rank.
    /// Rank I: Show RankI only
    /// Rank II: Show RankI + RankII
    /// Rank III: Show all three
    /// </summary>
    private void ConfigureRankVisibility(PowerUpRank rank)
    {
        if (rankI != null) rankI.SetActive(rank >= PowerUpRank.I);
        if (rankII != null) rankII.SetActive(rank >= PowerUpRank.II);
        if (rankIII != null) rankIII.SetActive(rank >= PowerUpRank.III);
    }

    /// <summary>
    /// Returns the UI color for a given rarity tier.
    /// Common → White
    /// Rare → Blue
    /// Epic → Purple
    /// Legendary → Orange/Gold
    /// </summary>
    public static Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Rare => new Color(0.3f, 0.5f, 1f),        // Blue
            Rarity.Epic => new Color(0.7f, 0.3f, 1f),        // Purple
            Rarity.Legendary => new Color(1f, 0.6f, 0f),     // Orange/Gold
            _ => Color.white                                  // Common (white)
        };
    }
}
