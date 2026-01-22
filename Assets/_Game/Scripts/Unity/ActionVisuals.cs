using UnityEngine;
using PocketSquire.Arena.Core;

/// <summary>
/// ScriptableObject that holds visual and audio data for a specific action type.
/// Create one of these for each action type (Attack, Block, etc.) and assign
/// animations, sounds, and duration in the Unity Inspector.
/// </summary>
[CreateAssetMenu(fileName = "NewActionVisuals", menuName = "Game/Action Visuals")]
public class ActionVisuals : ScriptableObject
{
    [Tooltip("The action type this visual configuration applies to")]
    public ActionType actionType;
    
    [Header("Animations")]
    [Tooltip("The trigger name to call on the actor's Animator")]
    public string actorAnimationTrigger;
    
    [Tooltip("The trigger name to call on the target's Animator (e.g., 'Hit')")]
    public string targetAnimationTrigger;
    
    [Header("Audio")]
    [Tooltip("Sound effect to play when the action executes")]
    public AudioClip soundEffect;
    
    [Header("Timing")]
    [Tooltip("How long to wait before applying the effect and completing the action")]
    public float duration = 0.5f;
}
