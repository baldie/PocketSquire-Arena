using UnityEngine;
using UnityEngine.UI;
using System;
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
    public GameObject powerupSelectionDialog;
    public Image playerImage;
    public ParticleSystem powerUpParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openingSound;

    [Header("PowerUp Options")]
    public PowerUpButtonScript powerUpOptionA;
    public PowerUpButtonScript powerUpOptionB;
    public PowerUpButtonScript powerUpOptionC;

    private Action _onLootCompleted;
    private Button chestButton;

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

        // Auto-select first option
        if (EventSystem.current != null && powerUpOptionA != null)
        {
            EventSystem.current.SetSelectedGameObject(powerUpOptionA.gameObject);
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
        mySequence.AppendCallback(() => chestImage.sprite = openedChestSprite);
        mySequence.Append(chestImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f));
        mySequence.OnComplete(() => {
            powerupSelectionDialog.SetActive(true);
            powerupSelectionDialog.transform.localScale = Vector3.zero;
            powerupSelectionDialog.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        });
    }

    private void SelectPowerUp(PowerUp powerUp)
    {
        // Add to collection
        if (GameState.CurrentRun != null)
        {
            GameState.CurrentRun.PowerUps.Add(powerUp);
            Debug.Log($"Selected PowerUp: {powerUp.DisplayName}");
        }

        // Hide loot window
        powerupSelectionDialog.SetActive(false);

        // Close and hide chest
        chestImage.gameObject.SetActive(false);
        _chestIsOpen = false;
        chestImage.sprite = closedChestSprite;

        ApplyPowerUpEffect();
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

        // 4. Final Cleanup & Invoke
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
}
