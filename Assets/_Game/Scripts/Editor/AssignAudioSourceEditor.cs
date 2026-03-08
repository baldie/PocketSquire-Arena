using UnityEngine;
using UnityEditor;
using PocketSquire.Arena.Unity.UI;

public static class AssignAudioSourceEditor
{
    [MenuItem("Tools/AssignAudioSourceToPerkList")]
    public static void AssignAudioSource()
    {
        var controllers = Object.FindObjectsByType<AcquiredPerkListController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var audioGo = GameObject.Find("UIAudio");
        
        if (audioGo == null)
        {
            Debug.LogError("Could not find UIAudio GameObject in scene.");
            return;
        }

        var source = audioGo.GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError("Could not find AudioSource on UIAudio.");
            return;
        }

        foreach (var controller in controllers)
        {
            var serializedObject = new SerializedObject(controller);
            var prop = serializedObject.FindProperty("audioSource");
            if (prop != null)
            {
                prop.objectReferenceValue = source;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
                Debug.Log($"Assigned AudioSource to {controller.gameObject.name}");
            }
        }
    }
}
