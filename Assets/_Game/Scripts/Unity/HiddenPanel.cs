using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

public class HiddenPanel : MonoBehaviour
{
    [SerializeField] private GameObject debugPanel;
    [Header("Debug Buttons")]
    [SerializeField] private Button healButton;
    [SerializeField] private Button killMonsterButton;
    [SerializeField] private Button rerollPowerUpsButton;
    [SerializeField] private Button toggleGender;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_EDITOR
        if (debugPanel != null)
        {
            debugPanel.SetActive(true);
        }
        
        if (healButton != null)
        {
            healButton.onClick.AddListener(() => {
                if (GameState.Player != null)
                {
                    GameState.Player.Heal(100);
                    Debug.Log("Player Healed");
                }
            });
        }

        if (killMonsterButton != null)
        {
            killMonsterButton.onClick.AddListener(() => {
                 if (GameState.Battle != null && GameState.Battle.Player2 != null)
                 {
                     GameState.Battle.Player2.TakeDamage(9999);
                     Debug.Log("Monster Killed");
                 }
            });
        }

        if (rerollPowerUpsButton != null)
        {
            rerollPowerUpsButton.onClick.AddListener(() => {
                var lootScript = FindFirstObjectByType<LootScript>();
                if (lootScript != null)
                {
                     lootScript.Reroll();
                     Debug.Log("Rerolled PowerUps");
                }
            });
        }

        if (toggleGender != null)
        {
            toggleGender.onClick.AddListener(() => {
                if (GameState.Player != null)
                {
                    GameState.Player.Gender = GameState.Player.Gender == Player.Genders.m ? Player.Genders.f : Player.Genders.m;
                    Debug.Log("Toggled Gender to " + GameState.Player.Gender);

                    var playerSprite = GameObject.Find("PlayerSprite");
                    if (playerSprite != null)
                    {
                        var playerImage = playerSprite.GetComponent<Image>();
                        if (playerImage != null)
                        {
                            var loadedSprite = GameAssetRegistry.Instance.GetSprite(GameState.Player.GetSpriteId());
                            if (loadedSprite != null)
                            {
                                playerImage.sprite = loadedSprite;
                            }
                        }
                    }
                }
            });
        }
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
