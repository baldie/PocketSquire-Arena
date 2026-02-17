using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core.PowerUps;

public class PowerUpHudController : MonoBehaviour
{
    [Header("Prefab Reference")]
    [Tooltip("PowerUpIcon prefab to instantiate for each power-up")]
    public GameObject powerUpIconPrefab;

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

            CreatePowerUpIcon(powerUpIconPrefab, powerUp, targetParent);
        }
    }

    /// <summary>
    /// Creates a PowerUp icon by instantiating the prefab and configuring it.
    /// </summary>
    /// <param name="prefab">PowerUpIcon prefab to instantiate</param>
    /// <param name="powerUp">PowerUp data to configure the icon with</param>
    /// <param name="parent">Parent transform to attach the icon to</param>
    /// <returns>The instantiated icon GameObject</returns>
    public static GameObject CreatePowerUpIcon(GameObject prefab, PowerUp powerUp, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("[PowerUpHudController] PowerUpIcon prefab is null");
            return null;
        }

        if (parent == null)
        {
            Debug.LogWarning($"[PowerUpHudController] Parent is null for power-up {powerUp?.DisplayName}");
            return null;
        }

        if (powerUp == null)
        {
            Debug.LogWarning("[PowerUpHudController] PowerUp is null");
            return null;
        }

        // Instantiate the prefab
        var iconObj = Instantiate(prefab, parent, false);
        iconObj.name = $"PowerUpIcon_{powerUp.UniqueKey}";

        // Get the PowerUpIconController and configure it
        var controller = iconObj.GetComponent<PowerUpIconController>();
        if (controller != null)
        {
            controller.Configure(powerUp);
        }
        else
        {
            Debug.LogError("[PowerUpHudController] PowerUpIconController component not found on prefab");
        }

        return iconObj;
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
