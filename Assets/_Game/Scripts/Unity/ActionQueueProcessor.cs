using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PocketSquire.Arena.Core;
using TMPro;
using DG.Tweening;

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

    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private Coroutine currentActionCoroutine = null;

    /// <summary>
    /// Event fired when an action completes. Can be used to trigger turn changes.
    /// </summary>
    public event System.Action<IGameAction> OnActionComplete;

    /// <summary>
    /// Returns true if the queue is currently processing an action.
    /// </summary>
    public bool IsProcessing => currentActionCoroutine != null;

    /// <summary>
    /// Returns the number of actions currently in the queue.
    /// </summary>
    public int QueueCount => actionQueue.Count;

    void Awake()
    {
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
        Debug.Log($"Action enqueued: {action.Type} by {action.Actor.Name}");
    }

    private IEnumerator ProcessAction(IGameAction action)
    {
        Debug.Log($"Processing action: {action.Type} by {action.Actor.Name} -> {action.Target.Name}");

        float duration = action.Actor?.GetActionDuration(action.Type) ?? 0.5f;

        // 1. Trigger Actor Effects
        PlayActionEffects(action.Actor, action.Type);

        // 2. Wait for mid-point (impact)
        yield return new WaitForSeconds(duration * 0.5f);

        // 3. Trigger Target Hit Effects (if it was an attack)
        if (action.Type == ActionType.Attack && action.Target != null)
        {
            if (action.Target.IsDefending)
            {
                PlayDefendEffects(action as AttackAction);
            }
            else
            {
                PlayHitEffects(action as AttackAction);
            }
        }

        // 4. Wait for remainder of animation
        yield return new WaitForSeconds(duration * 0.5f);

        // 5. Apply Effect and Cleanup
        action.ApplyEffect();
        Debug.Log($"Action complete: {action.Type} finished. {action.Target?.Name} now has {action.Target?.Health} HP.");

        OnActionComplete?.Invoke(action);
        currentActionCoroutine = null;
    }

    private void PlayActionEffects(Entity actor, ActionType type)
    {
        if (actor == null) return;

        // Sound
        string soundId = actor.GetActionSoundId(type);
        AudioClip clip = !string.IsNullOrEmpty(soundId) ? assetRegistry?.GetSound(soundId) : null;
        
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Animation / Visuals
        TriggerVisuals(actor, type);
    }

    private void PlayHitEffects(AttackAction action)
    {
        if (action == null) return;
        var target = action.Target;
        if (target == null) return;

        // Sound
        string hitSoundId = target.GetHitSoundId();
        AudioClip clip = !string.IsNullOrEmpty(hitSoundId) ? assetRegistry?.GetSound(hitSoundId) : null;
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Animation / Visuals
        TriggerVisuals(target, ActionType.Hit);

        // Show Damage Number
        var textControl = target is Player ? playerEffectText : monsterEffectText;
        ShowNumberEffect(textControl, action.Damage, Color.red);
    }

    private void PlayDefendEffects(AttackAction action)
    {
        if (action == null) return;
        var target = action.Target;
        if (target == null) return;

        // Sound
        string defendSoundId = target.DefendSoundId;
        AudioClip clip = !string.IsNullOrEmpty(defendSoundId) ? assetRegistry?.GetSound(defendSoundId) : null;
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);

        // Show Damage Number
        var textControl = target is Player ? playerEffectText : monsterEffectText;
        ShowNumberEffect(textControl, action.Damage, Color.white);
    }

    private void TriggerVisuals(Entity entity, ActionType actionType)
    {
        if (entity == null) return;

        GameObject go = FindGameObjectForEntity(entity);
        if (go != null)
        {
            // Trigger Animator (if used for generic states like "Hit" or "Idle")
            var animator = go.GetComponent<Animator>();
            string triggerName = actionType.ToString();
            if (animator != null) animator.SetTrigger(triggerName);

            // Sprite Swapping Logic
            var imgComponent = go.GetComponent<UnityEngine.UI.Image>();
            if (imgComponent == null) return;
            
            if (actionType == ActionType.Hit)
            {
                // 1. Shake
                var rectTransform = go.GetComponent<RectTransform>();
                rectTransform.DOShakeAnchorPos(0.4f, 15f, 20, 90f);

                // 2. Flash and show hit sprite
                if (imgComponent.material != null)
                {
                    var hitSprite = assetRegistry.GetSprite(entity.HitSpriteId);
                    if (hitSprite == null) {
                        Debug.LogWarning($"Hit sprite `{entity.HitSpriteId}` not found for {entity.Name}");
                    }
                    else
                    {
                        Debug.Log($"Hit sprite `{entity.HitSpriteId}` found for {entity.Name}");
                    }
                    var idleSprite = imgComponent.sprite;

                    // Kill previous flash if it's still running
                    DOTween.Kill(imgComponent.material);
                    DOTween.Sequence()
                        .Append(imgComponent.material.DOFloat(1f, "_FlashAmount", 0))   // Flash white
                        .AppendInterval(0.08f)                                // Hold
                        .Append(imgComponent.material.DOFloat(0f, "_FlashAmount", 0))   // Flash back
                        .AppendCallback(() => imgComponent.sprite = hitSprite) // Swap sprite
                        .AppendInterval(0.08f)                                // Hold
                        .OnComplete(() => {
                            imgComponent.sprite = idleSprite;
                        })
                        .SetTarget(imgComponent.material);
                }
            } 
            else if (actionType == ActionType.Attack) 
            {
                var attackSprite = assetRegistry.GetSprite(entity.AttackSpriteId);
                if (attackSprite == null) {
                    Debug.LogWarning($"Attack sprite `{entity.AttackSpriteId}` not found for {entity.Name}");
                }
                var idleSprite = imgComponent.sprite;
                DOTween.Sequence()
                    .AppendCallback(() => imgComponent.sprite = attackSprite) // Swap sprite
                    .AppendInterval(0.25f)                                // Hold
                    .OnComplete(() => {
                        imgComponent.sprite = idleSprite;
                    })
                    .SetTarget(imgComponent.material);
            }  
            else if (actionType == ActionType.Defend) 
            {
                var defendSprite = assetRegistry.GetSprite(entity.DefendSpriteId);
                if (defendSprite == null) {
                    Debug.LogWarning($"Defend sprite `{entity.DefendSpriteId}` not found for {entity.Name}");
                }
                
                // For Defend, we swap to the sprite. 
                // IMPORTANT: The sprite should stay as long as IsDefending is true, 
                // BUT determining when to swap back is tricky in this method.
                // For now, let's just show it briefly for the action duration to signify the "Action" of defending.
                // If we want a persistent stance, that would require state management in Update() or a separate system.
                // Given the current architecture, let's treat it like Attack - a temporary visual cue.
                
                var idleSprite = imgComponent.sprite; // Assume current is idle/base
                
                DOTween.Sequence()
                   .AppendCallback(() => imgComponent.sprite = defendSprite)
                   .AppendInterval(1.5f) // Defend action duration context
                   .OnComplete(() => {
                       // Only revert if we are done? 
                       // Actually, let's revert to keep it safe from getting stuck.
                       imgComponent.sprite = idleSprite;
                   })
                   .SetTarget(imgComponent.material);
            }
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
        if (entity is Player) return GameObject.Find(playerObjectName);
        if (entity is Monster) return GameObject.Find(monsterObjectName);
        return null;
    }
}
