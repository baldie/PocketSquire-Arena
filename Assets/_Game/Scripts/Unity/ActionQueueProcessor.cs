using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.UI.LevelUp;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using PocketSquire.Unity.UI;


/// <summary>
/// Manages a queue of game actions and processes them sequentially using coroutines.
/// Attach this to a GameObject in your Arena scene.
/// </summary>
public class ActionQueueProcessor : MonoBehaviour
{
    [Tooltip("Reference to the game asset registry for loading sounds")]
    public GameAssetRegistry assetRegistry;
    
    [Tooltip("AudioSource to play sound effects")]
    public AudioSource audioSource;

    [Header("Scene Object Lookups")]
    public string playerObjectName = "PlayerSprite";
    public string monsterObjectName = "MonsterSprite";
    public TextMeshProUGUI monsterEffectText;
    public TextMeshProUGUI playerEffectText;
    public Image playerHealthBarActual;
    public Image playerHealthBarGhost;
    public Image monsterHealthBarActual;
    public Image monsterHealthBarGhost;
    public Image playerUsedItemSprite;
    public Canvas arenaMenuPanel;
    public ConfirmationDialog confirmationDialog;
    public GameObject levelUpBackground;


    [Header("Audio")]
    public AudioClip crowd_pleased;
    public AudioClip crowd_displeased;
    public AudioClip player_win;
    public AudioClip player_lose;
        
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private Coroutine currentActionCoroutine = null;
    private bool _isWaitingForUser = false;
    private bool _yieldCancelled = false;
    private enum HealthBarAnimationType { Snap, Shake, None }


    /// <summary>
    /// Event fired when an action completes. Can be used to trigger turn changes.
    /// </summary>
    public event System.Func<IGameAction, IGameAction> OnActionComplete;

    /// <summary>
    /// Event fired when the processor has finished all actions in the queue.
    /// </summary>
    public event System.Action OnProcessingFinished;

    /// <summary>
    /// Returns true if the queue is currently processing an action.
    /// </summary>
    public bool IsProcessing => currentActionCoroutine != null;

    /// <summary>
    /// Returns the number of actions currently in the queue.
    /// </summary>
    public int QueueCount => actionQueue.Count;

    private GameObject _playerGO;
    private GameObject _monsterGO;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.PlayOneShot(crowd_pleased);

        // Cache GameObjects
        _playerGO = GameObject.Find(playerObjectName);
        _monsterGO = GameObject.Find(monsterObjectName);

        // Make the healthbars snap to their current values
        UpdateHealth(playerHealthBarActual, playerHealthBarGhost, GameState.Battle.Player1.Health, GameState.Battle.Player1.MaxHealth, HealthBarAnimationType.Snap);
        UpdateHealth(monsterHealthBarActual, monsterHealthBarGhost, GameState.Battle.Player2.Health, GameState.Battle.Player2.MaxHealth, HealthBarAnimationType.Snap);
    }

    void Update()
    {
        // If not processing and queue has items, start processing the next action
        if (currentActionCoroutine == null && actionQueue.Count > 0)
        {
            var nextAction = actionQueue.Dequeue();
            currentActionCoroutine = StartCoroutine(ProcessAction(nextAction));
        }
    }

    /// <summary>
    /// Adds an action to the queue. It will be processed when the queue reaches it.
    /// </summary>
    public void EnqueueAction(IGameAction action)
    {
        if (action == null)
        {
            Debug.LogWarning("Attempted to enqueue a null action.");
            return;
        }
        actionQueue.Enqueue(action);
    }

    private IEnumerator ProcessAction(IGameAction action)
    {
        _yieldCancelled = false;
        // 1. Trigger Actor Effects
        action.ApplyEffect();

        PlayActionEffects(action);

        // Wait if visuals/logic requested user input (e.g. Yield Dialog)
        while (_isWaitingForUser)
        {
            yield return null;
        }

        // 2. Wait for mid-point (impact)

        yield return new WaitForSeconds(0.5f);       

        // 3. Trigger Target Hit Effects (if it was an attack or special attack)
        if ((action.Type == ActionType.Attack || action.Type == ActionType.SpecialAttack) && action.Target != null)
        {
            // Extract damage from either AttackAction or SpecialAttackAction
            int damage = action switch
            {
                AttackAction atk => atk.Damage,
                SpecialAttackAction spAtk => spAtk.Damage,
                _ => 0
            };

            if (action.Target.IsDefending)
            {
                PlayDefendEffects(action.Target, damage);
            }
            else
            {
                PlayHitEffects(action.Target, damage);
            }
            // 4. Wait for remainder of animation
            yield return new WaitForSeconds(0.5f);
        }       

        // 6. Determine next action
        if (!_yieldCancelled)
        {
            IGameAction result = OnActionComplete?.Invoke(action);
            if (result != null)
            {
                EnqueueAction(result);
            }
        }
        currentActionCoroutine = null;

        // If no more actions are queued, notify listeners (e.g., BattleManager to show menu)
        if (actionQueue.Count == 0)
        {
            OnProcessingFinished?.Invoke();
        }
    }

    private void PlayActionEffects(IGameAction action)
    {
        if (action?.Actor == null) return;

        // Sound
        string soundId = action.Actor.GetActionSoundId(action.Type);
        AudioClip clip = !string.IsNullOrEmpty(soundId) ? assetRegistry?.GetSound(soundId) : null;
        
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Animation / Visuals
        TriggerVisuals(action.Actor, action.Type, action);
    }

    private void PlayHitEffects(Entity target, int damage)
    {
        if (target == null) return;

        // Sound
        string hitSoundId = target.GetHitSoundId();
        AudioClip clip = !string.IsNullOrEmpty(hitSoundId) ? assetRegistry?.GetSound(hitSoundId) : null;
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Show Damage Number
        var textControl = target is Player ? playerEffectText : monsterEffectText;
        ShowNumberEffect(textControl, damage, Color.red);

        var healthbarActual = target is Player ? playerHealthBarActual : monsterHealthBarActual;
        var healthbarGhost = target is Player ? playerHealthBarGhost : monsterHealthBarGhost;
        UpdateHealth(healthbarActual, healthbarGhost, target.Health, target.MaxHealth, HealthBarAnimationType.Shake);
        
        // Animation / Visuals
        TriggerVisuals(target, ActionType.Hit);
    }

    private void PlayDefendEffects(Entity target, int damage)
    {
        if (target == null) return;

        // Sound
        string defendSoundId = target.DefendSoundId;
        AudioClip clip = !string.IsNullOrEmpty(defendSoundId) ? assetRegistry?.GetSound(defendSoundId) : null;
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Show Damage Number
        var textControl = target is Player ? playerEffectText : monsterEffectText;
        ShowNumberEffect(textControl, damage, Color.white);

        // Update health bar
        var healthbarActual = target is Player ? playerHealthBarActual : monsterHealthBarActual;
        var healthbarGhost = target is Player ? playerHealthBarGhost : monsterHealthBarGhost;
        UpdateHealth(healthbarActual, healthbarGhost, target.Health, target.MaxHealth, HealthBarAnimationType.Shake);
    }

    private void TriggerVisuals(Entity entity, ActionType actionType, IGameAction action = null)
    {
        if (entity == null) return;

        GameObject go = FindGameObjectForEntity(entity);
        if (go == null) return;

        var imgComponent = go.GetComponent<Image>();
        if (imgComponent == null) return;

        switch (actionType)
        {
            case ActionType.Hit:
                HandleHitVisuals(go, imgComponent, entity);
                break;
            case ActionType.Attack:
                HandleSpriteSwap(imgComponent, entity.AttackSpriteId, 0.25f);
                break;
            case ActionType.SpecialAttack:
                HandleSpriteSwap(imgComponent, entity.SpecialAttackSpriteId, 0.25f);
                break;
            case ActionType.Defend:
                HandleSpriteSwap(imgComponent, entity.DefendSpriteId, 2.0f);
                break;
            case ActionType.Win:
                HandleWinVisuals(imgComponent, entity);
                break;
            case ActionType.Lose:
                HandleLoseVisuals(imgComponent, entity);
                break;
            case ActionType.Yield:
                HandleYield(imgComponent, entity);
                break;
            case ActionType.Item:
                HandleItemEffects(imgComponent, entity, action as ItemAction);
                break;
        }
    }

    private void HandleYield(Image img, Entity entity)
    {
        _isWaitingForUser = true;
        Debug.Log("Showing yield confirmation dialog...");
        ConfirmationDialog.Show(
            confirmationDialog,
            "Give up now?",
            () => SceneManager.LoadScene("Town"),
            () => {
                _isWaitingForUser = false;
                _yieldCancelled = true;
            }
        );
    }

    private void HandleItemEffects(Image img, Entity entity, ItemAction itemAction)
    {
        if (itemAction == null || itemAction.ItemData == null) return;
        
        // Visual: Use Player's ItemSpriteId (or generic if monster, but monsters use different logic usually)
        if (entity is Player player)
        {
            HandleSpriteSwap(img, player.ItemSpriteId, 1.0f);
        }
        
        // Audio: Play item's sound effect
        string soundId = itemAction.ItemData.SoundEffect;
        if (!string.IsNullOrEmpty(soundId))
        {
            AudioClip clip = assetRegistry?.GetSound(soundId);
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"[ActionQueueProcessor] No sound effect found for: {itemAction.ItemData.SoundEffect}");
            }
        }
        else
        {
            Debug.LogWarning($"[ActionQueueProcessor] No sound effect found for item: {itemAction.ItemData.Id}");
        }

        // Show item above character
        if (playerUsedItemSprite != null)
        {
            Sprite itemSprite = assetRegistry.GetSprite(itemAction.ItemData.Sprite);
            if (itemSprite != null)
            {
                playerUsedItemSprite.sprite = itemSprite;
                playerUsedItemSprite.color = Color.white;
                Vector3 originalPos = playerUsedItemSprite.transform.localPosition;

                DOTween.Sequence()
                    .Append(playerUsedItemSprite.transform.DOLocalMoveY(originalPos.y + 50f, 1f).SetEase(Ease.OutQuad))
                    .Join(playerUsedItemSprite.DOFade(0f, 1f).SetEase(Ease.InQuad))
                    .OnComplete(() => {
                        playerUsedItemSprite.transform.localPosition = originalPos;
                        playerUsedItemSprite.sprite = assetRegistry.GetSprite("transparent_pixel");
                        playerUsedItemSprite.color = Color.white;
                    });
            }
        }

        // Update health bar
        UpdateHealth(playerHealthBarActual, playerHealthBarGhost, entity.Health, entity.MaxHealth, HealthBarAnimationType.Snap);
    }

    private void HandleLoseVisuals(Image img, Entity entity)
    {
        SetSprite(img, entity.DefeatSpriteId);

        // Sound
        if (audioSource != null){
            audioSource.PlayOneShot(player_lose);
            audioSource.PlayOneShot(crowd_displeased);
        }

        showArenaMenu(false);
    }

    private void HandleHitVisuals(GameObject go, Image img, Entity entity)
    {
        var rectTransform = go.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.DOShakeAnchorPos(0.4f, 15f, 20, 90f);
        }

        if (img.material == null) return;

        Sprite hitSprite = assetRegistry.GetSprite(entity.HitSpriteId);
        if (hitSprite == null)
        {
            Debug.LogWarning($"Hit sprite `{entity.HitSpriteId}` not found for {entity.Name}");
            return;
        }

        Sprite idleSprite = img.sprite;
        DOTween.Kill(img.material);
        DOTween.Sequence()
            .Append(img.material.DOFloat(1f, "_FlashAmount", 0))
            .AppendInterval(0.08f)
            .Append(img.material.DOFloat(0f, "_FlashAmount", 0))
            .AppendCallback(() => img.sprite = hitSprite)
            .AppendInterval(0.08f)
            .OnComplete(() => img.sprite = idleSprite)
            .SetTarget(img.material);
    }

    private void HandleSpriteSwap(Image img, string spriteId, float duration)
    {
        Sprite newSprite = assetRegistry.GetSprite(spriteId);
        if (newSprite == null)
        {
            Debug.LogWarning($"Sprite `{spriteId}` not found.");
            return;
        }

        Sprite originalSprite = img.sprite;
        DOTween.Sequence()
            .AppendCallback(() => img.sprite = newSprite)
            .AppendInterval(duration)
            .OnComplete(() => img.sprite = originalSprite)
            .SetTarget(img.material);
    }

    private void UpdateHealth(Image healthBarActual, Image healthBarGhost, int currentHealth, int maxHealth, HealthBarAnimationType animationType)
    {
        float targetFill = (float)currentHealth / maxHealth;

        // 1. Shake health bar
        if (animationType == HealthBarAnimationType.Shake)
        {
            healthBarActual.transform.parent.DOKill(true); // Complete previous shake if hit again
            healthBarActual.transform.parent.DOShakePosition(0.3f, strength: 10f, vibrato: 20);
        }
        
        // 2. Snap the actual health bar instantly
        healthBarActual.fillAmount = targetFill;

        // 3. Animate the ghost bar
        // Only start a new tween if the ghost is actually further ahead than the actual bar
        if (healthBarGhost.fillAmount > targetFill && animationType != HealthBarAnimationType.Snap) 
        {
            // Complete: false ensures we don't snap to the end before restarting
            healthBarGhost.DOKill(false); 

            healthBarGhost.DOFillAmount(targetFill, 0.5f)
                .SetDelay(0.5f)
                .SetEase(Ease.OutQuad);
        }
        else 
        {
            // If healing, just snap the ghost bar to match
            healthBarGhost.fillAmount = targetFill;
        }
    }

    private void SetSprite(Image img, string spriteId)
    {
        Sprite newSprite = assetRegistry.GetSprite(spriteId);
        if (newSprite != null) img.sprite = newSprite;
    }

    private void HandleWinVisuals(Image playerImg, Entity player)
    {
        var monsterGO = FindGameObjectForEntity(GameState.Battle.Player2);
        if (monsterGO == null) return;

        var monsterImage = monsterGO.GetComponent<Image>();
        if (monsterImage == null) return;

        var monsterRect = monsterImage.rectTransform;

        var deathSeq = DOTween.Sequence();
        deathSeq.AppendCallback(() => {
            string soundId = GameState.Battle.Player2.DefeatSoundId;
            AudioClip clip = !string.IsNullOrEmpty(soundId) ? assetRegistry?.GetSound(soundId) : null;
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
        });
        deathSeq.Append(monsterImage.DOFade(0f, 0.8f).SetEase(Ease.OutQuint));
        deathSeq.Join(monsterRect.DOAnchorPosY(monsterRect.anchoredPosition.y - 50f, 1.8f, true));

        deathSeq.OnComplete(() =>
        {
            SetSprite(playerImg, GameState.Battle.Player1.WinSpriteId);
            if (monsterGO.transform.parent != null)
                monsterGO.transform.parent.localScale = Vector3.zero;
        });

        deathSeq.SetLink(monsterGO);

        if (audioSource != null) audioSource.PlayOneShot(player_win);

        if (GameState.Player.CanLevelUp())
        {
            Debug.Log("Player can level up");
            if (levelUpBackground != null)
            {
                var presenter = levelUpBackground.GetComponent<LevelUpPresenter>();

                // Show the arena menu after the player has leveled up.
                LevelUpPresenter.Show(levelUpBackground, presenter, () => showArenaMenu(true));
            } else {
                Debug.LogError("LevelUpBackground not found!");
            }
        } else {
            showArenaMenu(true);
        }
    }

    private void showArenaMenu(bool canContinue)
    {
        if (arenaMenuPanel == null){
            Debug.LogError("ArenaMenu not found!");
            return;
        }

        arenaMenuPanel.gameObject.SetActive(true);

        // Get the Button components of the arena menu
        var btnNextOpponent = arenaMenuPanel.transform.GetChild(0).GetComponent<Button>();
        var btnLeaveArena = arenaMenuPanel.transform.GetChild(1).GetComponent<Button>();

        // Enable / disable based on if the player can continue onwards
        btnNextOpponent.interactable = canContinue;

        if (canContinue) {   
            EventSystem.current.SetSelectedGameObject(btnNextOpponent.gameObject);
        } else {
            // If the next opponent button is disabled, we focus the leave button
            EventSystem.current.SetSelectedGameObject(btnLeaveArena.gameObject);
        }
    }

    // Use for big hits
    public void HitStop()
    {
        // Temporarily set timeScale to 0.05 (near frozen) then snap back
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.05f, 0.01f)
            .OnComplete(() => {
                DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.1f)
                        .SetUpdate(true); // Must be true so it works while time is slow!
            }).SetUpdate(true);
    }

    public void ShowNumberEffect(TextMeshProUGUI textControl, int amount, Color textColor)
    {
        textControl.text = amount.ToString();
        textControl.color = textColor;

        // Reset properties before starting (important if reusing the object)
        textControl.alpha = 0;
        textControl.transform.localScale = Vector3.zero;
        Vector3 originalPos = textControl.transform.localPosition;

        // Create a Sequence
        Sequence effectSequence = DOTween.Sequence();

        // 1 & 2. Become visible and "Pop up"
        effectSequence.Append(textControl.DOFade(1, 0.1f));
        effectSequence.Join(textControl.transform.DOScale(4f, 0.2f).SetEase(Ease.OutBack));
        effectSequence.Join(textControl.transform.DOLocalMoveY(originalPos.y + 150f, 0.3f).SetEase(Ease.OutQuad));

        // 3. Fall back down and bounce twice
        // We use Ease.OutBounce to handle the "bounce" automatically
        effectSequence.Append(textControl.transform.DOLocalMoveY(originalPos.y, 0.5f).SetEase(Ease.OutBounce));

        // 4. Fade away
        effectSequence.AppendInterval(0.5f); // Small pause so the player can read it
        effectSequence.Append(textControl.DOFade(0, 0.3f));
        
        // Optional: Reset scale back to 0 so it's ready for next time
        effectSequence.OnComplete(() => textControl.transform.localScale = Vector3.zero);
    }

    private GameObject FindGameObjectForEntity(Entity entity)
    {
        if (entity is Player) return _playerGO ?? (_playerGO = GameObject.Find(playerObjectName));
        if (entity is Monster) return _monsterGO ?? (_monsterGO = GameObject.Find(monsterObjectName));
        return null;
    }
}
