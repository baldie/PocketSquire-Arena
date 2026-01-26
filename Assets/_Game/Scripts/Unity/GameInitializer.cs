using UnityEngine;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] 
        private PocketSquire.Arena.Unity.LevelUp.ProgressionSchedule _progressionAsset; 

        void Awake()
        {
            if (_progressionAsset != null)
            {
                GameWorld.Progression = _progressionAsset.Logic;
            }
            else
            {
                Debug.LogError("Progression Asset is missing from GameInitializer!");
            }
        }
    }
}