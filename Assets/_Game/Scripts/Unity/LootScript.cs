using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.PowerUps;
using DG.Tweening;

public class LootScript : MonoBehaviour
{
    [Header("UI")]
    public Image chestImage;
    public Sprite openedChestSprite;
    public Sprite closedChestSprite;
    public Sprite highlightedChestSprite;
    public GameObject powerupSelectionDialog;
    public Button rerollPowerUpButton;
    public Image playerImage;
    public ParticleSystem powerUpParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openingSound;

    [Header("PowerUp Options")]
    public PowerUpButtonScript powerUpOptionA;
    public PowerUpButtonScript powerUpOptionB;
    public PowerUpButtonScript powerUpOptionC;

    [Header("PowerUp Icon Prefab")]
    [Tooltip("PowerUpIcon prefab to instantiate when animating power-up selection")]
    public GameObject powerUpIconPrefab;

    private Action _onLootCompleted;
    private Button chestButton;
    private PowerUp _selectedPowerUp;

    private const int HEIGHT_WITH_REROLL_OPTION = 260;
    private const int HEIGHT_WITHOUT_REROLL_OPTION = 384;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        chestButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (chestButton != null)
        {
            chestButton.onClick.AddListener(OpenChest);
        }
    }

    void OnDisable()
    {
        if (chestButton != null)
        {
            chestButton.onClick.RemoveListener(OpenChest);
        }
    }

    public void ShowChest(Action onLootCompleted)
    {
        _onLootCompleted = onLootCompleted;
        
        // Reset UI state
        chestImage.gameObject.SetActive(true);
        chestImage.sprite = closedChestSprite;
        var state = chestButton.spriteState;
        state.highlightedSprite = highlightedChestSprite;
        state.pressedSprite = highlightedChestSprite;
        chestButton.spriteState = state;
        chestButton.interactable = true;

        GenerateAndPopulatePowerUps(true);
    }

    /// <summary>
    /// Generates and populates power-up options.
    /// </summary>
    /// <param name="autoSelectFirst">Whether to auto-select the first option.</param>
    private void GenerateAndPopulatePowerUps(bool autoSelectFirst)
    {
        bool hasHighRoller = GameState.Player?.ActivePerks?.Any(p => p.Id == "high_roller") ?? false;
        
        if (rerollPowerUpButton != null)
        {
            rerollPowerUpButton.gameObject.SetActive(hasHighRoller);
            rerollPowerUpButton.onClick.RemoveAllListeners();
            rerollPowerUpButton.onClick.AddListener(() =>
            {
                Reroll();
                rerollPowerUpButton.interactable = false;

                var rt = rerollPowerUpButton.GetComponent<RectTransform>();
                var cg = rerollPowerUpButton.GetComponent<CanvasGroup>();
                var seq = DOTween.Sequence();
                seq.Append(rt.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5, 0.5f));
                seq.Append(rt.DOScale(0f, 0.3f).SetEase(Ease.InBack));
                seq.Join(cg.DOFade(0f, 0.3f));
                seq.OnComplete(() => {
                    rerollPowerUpButton.gameObject.SetActive(false);
                    ResizeDialog(false);
                });
            });
        }

        ResizeDialog(hasHighRoller);
        Reroll();

        // Auto-select first option
        if (autoSelectFirst && EventSystem.current != null && powerUpOptionA != null)
        {
            EventSystem.current.SetSelectedGameObject(powerUpOptionA.gameObject);
        }
    }

    private void ResizeDialog(bool hasHighRoller)
    {
        if (powerupSelectionDialog != null)
        {
            var rectTransform = powerupSelectionDialog.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                var offsetMin = rectTransform.offsetMin;
                offsetMin.y = hasHighRoller ? HEIGHT_WITH_REROLL_OPTION : HEIGHT_WITHOUT_REROLL_OPTION;
                rectTransform.offsetMin = offsetMin;
            }
        }
    }

    public void Reroll()
    {
        // Generate PowerUps
        var context = new PowerUpFactory.PowerUpGenerationContext
        {
            ArenaLevel = GameState.CurrentRun?.ArenaRank ?? 1,
            PlayerLuck = GameState.Player?.Attributes.Luck ?? 0,
            PlayerHealthPercent = GetPlayerHealthPercent(),
            OwnedPowerUps = GameState.CurrentRun?.PowerUps ?? new PowerUpCollection()
        };

        // Use a persistent random if possible, otherwise new random
        var choices = PowerUpFactory.Generate(context, new System.Random());

        if (choices.Count >= 3)
        {
            if (powerUpOptionA != null) powerUpOptionA.LoadPowerUp(choices[0], SelectPowerUp);
            if (powerUpOptionB != null) powerUpOptionB.LoadPowerUp(choices[1], SelectPowerUp);
            if (powerUpOptionC != null) powerUpOptionC.LoadPowerUp(choices[2], SelectPowerUp);
        }
    }

    private bool _chestIsOpen = false;
    public void OpenChest()
    {
        if (_chestIsOpen) return;
        _chestIsOpen = true;
        
        if (audioSource != null && openingSound != null)
        {
            audioSource.PlayOneShot(openingSound);
        }

        var mySequence = DOTween.Sequence();
        mySequence.Append(chestImage.transform.DOPunchRotation(new Vector3(0, 0, 10), 0.2f)); 
        mySequence.AppendCallback(() => {
           chestImage.sprite = openedChestSprite;
           chestButton.interactable = false;
        });
        mySequence.Append(chestImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f));
        mySequence.OnComplete(() => {
            powerupSelectionDialog.SetActive(true);
            powerupSelectionDialog.transform.localScale = Vector3.zero;
            powerupSelectionDialog.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        });
    }

    private void SelectPowerUp(PowerUp powerUp)
    {
        // Store selected power-up for animation
        _selectedPowerUp = powerUp;

        // Add to collection
        if (GameState.CurrentRun != null)
        {
            GameState.CurrentRun.PowerUps.Add(powerUp);
            Debug.Log($"Selected PowerUp: {powerUp.DisplayName}");

            // Ensure we reference the persistent PowerUp instance from the collection
            // This guarantees the HUD icon updates automatically if rank increases later
            foreach (var p in GameState.CurrentRun.PowerUps.GetAll())
            {
                if (p.UniqueKey == powerUp.UniqueKey)
                {
                    _selectedPowerUp = p;
                    break;
                }
            }
        }

        // Hide loot window
        powerupSelectionDialog.SetActive(false);

        // Close and hide chest
        chestImage.gameObject.SetActive(false);
        _chestIsOpen = false;
        chestImage.sprite = closedChestSprite;

        ApplyImmediateSelectionEffects();
        ApplyPowerUpEffect();
    }

    private void ApplyImmediateSelectionEffects()
    {
        if (_selectedPowerUp?.Component is not UtilityComponent utilityComponent)
        {
            return;
        }

        if (GameState.Player == null || GameState.CurrentRun == null)
        {
            return;
        }

        // Loot is selected after a win and before the next battle starts, so utility
        // power-ups should pay out immediately for the battle that just ended.
        utilityComponent.ApplyToPlayer(GameState.Player, GameState.CurrentRun.ArenaRank);

        var actionQueueProcessor = FindFirstObjectByType<ActionQueueProcessor>();
        if (actionQueueProcessor != null)
        {
            actionQueueProcessor.RefreshBattleHud();
        }
    }

    public void ApplyPowerUpEffect()
    {
        // 1. Cleanup
        playerImage.transform.DOKill();
        playerImage.color = Color.white;

        // 2. Setup Sequence
        var seq = DOTween.Sequence();

        seq.OnStart(() => {
            if (powerUpParticles != null) {
                powerUpParticles.Play();
            }
        });

        // Color tint: White -> Yellow -> White (Total duration: 0.5s)
        seq.Append(playerImage.DOColor(Color.blue, 0.25f).SetLoops(2, LoopType.Yoyo));

        // 3. Stop particles after the color flash is done
        seq.AppendCallback(() => {
            if (powerUpParticles != null) {
                // 'false' means stop emitting new ones, but let existing ones finish their life
                powerUpParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        });

        // 4. Animate the power-up icon appearing in the power-up hud
        seq.AppendCallback(() => CreateAndAnimatePowerUpIcon());

        // 5. Final Cleanup & Invoke
        seq.OnComplete(() => {
            playerImage.color = Color.white;
            _onLootCompleted?.Invoke();
        });
    }

    private float GetPlayerHealthPercent()
    {
        if (GameState.Player == null || GameState.Player.MaxHealth == 0) return 1.0f;
        return (float)GameState.Player.Health / GameState.Player.MaxHealth;
    }

    /// <summary>
    /// Creates and animates a power-up icon appearing in the PowerUpHud.
    /// Only applies to player power-ups (not monster debuffs).
    /// </summary>
    private void CreateAndAnimatePowerUpIcon()
    {
        if (_selectedPowerUp == null || _selectedPowerUp.Component.ComponentType == PowerUpComponentType.MonsterDebuff)
        {
            return; // Skip animation for monster debuffs
        }

        // Find the PowerUpHudController in the scene
        var hudController = FindFirstObjectByType<PowerUpHudController>();
        if (hudController == null || hudController.playerPowerUpsParent == null)
        {
            Debug.LogWarning("[LootScript] PowerUpHudController or playerPowerUpsParent not found");
            return;
        }

        Transform parent = hudController.playerPowerUpsParent;

        // Check for existing lower-rank versions of this power-up and remove them
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            var selector = child.GetComponent<PowerUpSelector>();
            
            if (selector != null && selector.PowerUp != null)
            {
                // If same type (UniqueKey matches), remove it to replace with new version
                // Note: The underlying data might have already been updated by Add(), so we can't reliably check Rank < SelectedRank.
                // But since we are adding a new icon for this Key, we should always remove the old one.
                if (selector.PowerUp.UniqueKey == _selectedPowerUp.UniqueKey)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // Create the icon using the shared prefab-based method
        var iconObj = PowerUpHudController.CreatePowerUpIcon(powerUpIconPrefab, _selectedPowerUp, parent);
        if (iconObj == null)
        {
            Debug.LogWarning("[LootScript] Failed to create power-up icon");
            return;
        }

        // Add CanvasGroup for fade animation
        var canvasGroup = iconObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        // Set initial scale to zero for animation
        iconObj.transform.localScale = Vector3.zero;

        // Animate the icon appearing
        Sequence iconSeq = DOTween.Sequence();
        iconSeq.Append(iconObj.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
               .Join(canvasGroup.DOFade(1, 0.2f))
               .Append(iconObj.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.5f, 10, 1))
               .OnComplete(() => {
                   // Add a subtle idle "breath" animation
                   iconObj.transform.DOScale(1.05f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
               });
    }
}
