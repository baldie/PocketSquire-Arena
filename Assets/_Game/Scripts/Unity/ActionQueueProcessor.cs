using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PocketSquire.Arena.Core;

/// <summary>
/// Manages a queue of game actions and processes them sequentially using coroutines.
/// Attach this to a GameObject in your Arena scene.
/// </summary>
public class ActionQueueProcessor : MonoBehaviour
{
    [Tooltip("Reference to the game asset registry for loading sounds")]
    public GameAssetRegistry assetRegistry;
    
    [Tooltip("List of ActionVisuals assets for each action type")]
    public List<ActionVisuals> actionVisualsList;
    
    [Tooltip("AudioSource to play sound effects")]
    public AudioSource audioSource;

    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private Coroutine currentActionCoroutine = null;
    private Dictionary<ActionType, ActionVisuals> visualsMap;

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
        // Build a lookup map for action visuals
        visualsMap = new Dictionary<ActionType, ActionVisuals>();
        if (actionVisualsList != null)
        {
            foreach (var av in actionVisualsList)
            {
                if (av != null && !visualsMap.ContainsKey(av.actionType))
                {
                    visualsMap.Add(av.actionType, av);
                }
            }
        }
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

        // Look up visuals for this action type
        ActionVisuals visuals = null;
        visualsMap.TryGetValue(action.Type, out visuals);

        float duration = visuals?.duration ?? 0.5f;

        // Play sound effect if available
        if (visuals?.soundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(visuals.soundEffect);
        }
        else if (assetRegistry != null && action.Actor != null)
        {
            // Fallback: Try to get sound from registry using actor's attack sound ID
            var clip = assetRegistry.GetSound(action.Actor.AttackSoundId);
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // TODO: Play actor animation (if visuals.actorAnimation is set)
        // TODO: Play target animation (if visuals.targetAnimation is set)

        // Wait for the visual duration
        yield return new WaitForSeconds(duration);

        // Apply the actual game-state effect
        action.ApplyEffect();
        
        Debug.Log($"Action complete: {action.Type} finished. {action.Target.Name} now has {action.Target.Health} HP.");

        // Fire completion event
        OnActionComplete?.Invoke(action);

        currentActionCoroutine = null;
    }
}
