using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class PlayFromEntryScene
{
    static PlayFromEntryScene()
    {
        // Path to your starting scene relative to the Assets folder
        string scenePath = "Assets/_Game/Scenes/MainMenu.unity"; 
        
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        
        if (sceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
        }
    }
}