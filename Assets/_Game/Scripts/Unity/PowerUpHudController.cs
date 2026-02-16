using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core.PowerUps;

public class PowerUpHudController : MonoBehaviour
{
    [Header("Layout Group Parents")]
    [Tooltip("Parent for player-focused power-ups (buffs, loot, utility)")]
    public Transform playerPowerUpsParent;
    
    [Tooltip("Parent for monster-focused power-ups (debuffs)")]
    public Transform monsterPowerUpsParent;

    /// <summary>
    /// Populates the HUD with icons for all owned power-ups.
    /// Player buffs go to playerPowerUpsParent, monster debuffs go to monsterPowerUpsParent.
    /// </summary>
    public void PopulateHud(PowerUpCollection powerUps)
    {
        if (powerUps == null)
        {
            Debug.LogWarning("[PowerUpHudController] PowerUpCollection is null, skipping HUD population");
            return;
        }

        // Clear existing icons
        ClearParent(playerPowerUpsParent);
        ClearParent(monsterPowerUpsParent);

        // Create icon for each power-up
        foreach (var powerUp in powerUps.GetAll())
        {
            var targetParent = powerUp.Component.ComponentType == PowerUpComponentType.MonsterDebuff
                ? monsterPowerUpsParent
                : playerPowerUpsParent;

            CreatePowerUpIcon(powerUp, targetParent);
        }
    }

    private void CreatePowerUpIcon(PowerUp powerUp, Transform parent)
    {
        if (parent == null)
        {
            Debug.LogWarning($"[PowerUpHudController] Parent is null for power-up {powerUp.DisplayName}");
            return;
        }

        // Create GameObject with Image component
        var iconObj = new GameObject($"PowerUpIcon_{powerUp.UniqueKey}");
        iconObj.transform.SetParent(parent, false);

        // Add and configure RectTransform
        var rectTransform = iconObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(106, 106);

        // Add and configure Image
        var image = iconObj.AddComponent<Image>();
        var sprite = GameAssetRegistry.Instance.GetSprite(powerUp.Component.IconId);
        
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            GameAssetRegistry.Instance.LogAllSprites();
            Debug.LogWarning($"[PowerUpHudController] Icon sprite '{powerUp.Component.IconId}' not found for {powerUp.DisplayName}");
        }

        // Add PowerUpSelector for tooltip/description
        var selector = iconObj.AddComponent<PowerUpSelector>();
        selector.Initialize(powerUp);
    }

    private void ClearParent(Transform parent)
    {
        if (parent == null) return;

        // Destroy all children
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
