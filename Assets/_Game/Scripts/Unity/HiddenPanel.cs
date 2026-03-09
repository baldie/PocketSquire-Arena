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
    [SerializeField] private Button addGoldButton;
    [SerializeField] private Button addLevelButton;


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

        if (addGoldButton != null)
        {
            addGoldButton.onClick.AddListener(() => {
                if (GameState.Player != null)
                {
                    GameState.Player.Gold += 500;
                    Debug.Log("Added 500 Gold");
                }
            });
        }

        if (addLevelButton != null)
        {
            addLevelButton.onClick.AddListener(() => {
                if (GameState.Player != null)
                {
                    var player = GameState.Player;
                    var xpToLevel = GameWorld.Progression.GetXpToNextLevel(player.Experience);
                    player.GainExperience(xpToLevel);
                    Debug.Log("Player has enough experience to level up to " + player.Level);
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
