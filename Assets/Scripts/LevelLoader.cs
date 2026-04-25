using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro elements

public class LevelLoader : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private int maxLevel = 5;
    
    [Header("UI References")]
    [SerializeField] private RectTransform parentCanvas;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Level Icons")]
    [SerializeField] private Sprite unlockedIcon;
    [SerializeField] private Sprite disabledIcon;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    
    [Header("Grid Layout Settings")]
    [SerializeField] private float spacingX = 20f;
    [SerializeField] private float spacingY = 20f;
    [SerializeField] private float topOffset = 100f;
    [SerializeField] private float bottomOffset = 0f;

    void Start()
    {
        GenerateLevelGrid();
    }

    private void GenerateLevelGrid()
    {
        if (parentCanvas == null || buttonPrefab == null)
        {
            Debug.LogError("Parent Canvas or Button Prefab is missing in LevelLoader!");
            return;
        }

        // Load PlayerData using the SaveSystem script you provided
        PlayerData playerData = SaveSystem.LoadData();
        int unlockedLevel = (playerData != null) ? playerData.unlockedLevel : 1;

        // Calculate total grid dimensions in order to perfectly center the container
        int totalColumns = maxLevel;
        int totalRows = 2; // Zigzag pattern occupies 2 visual rows

        float totalGridWidth = (totalColumns - 1) * spacingX;
        float totalGridHeight = (totalRows - 1) * spacingY;

        // We center the grid by calculating the starting positions mapping to the extreme top-left.
        // We also apply the requested top and bottom offsets to naturally shift the vertical center!
        float verticalCenterOffset = (bottomOffset - topOffset) / 2f;
        float startX = -totalGridWidth / 2f;
        float startY = (totalGridHeight / 2f) + verticalCenterOffset;

        for (int i = 0; i < maxLevel; i++)
        {
            int levelIndex = i + 1; // Levels are typically 1-indexed (Level 1, Level 2, ...)

            // Zigzag pattern logic
            int col = i;
            int row = i % 2; // 0 for even indices (top), 1 for odd indices (bottom)

            float posX = startX + (col * spacingX);
            float posY = startY - (row * spacingY);

            // Instantiate the button inside our parent canvas
            GameObject levelBtnObj = Instantiate(buttonPrefab, parentCanvas);
            levelBtnObj.name = "LevelButton_" + levelIndex;

            // Set the position using the RectTransform
            RectTransform btnRect = levelBtnObj.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                // Ensure the button anchors to the center of the Canvas, so X/Y acts as a true center offset
                btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.pivot = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = new Vector2(posX, posY);
                btnRect.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Scale up by 1.5x
            }

            // Find TextMeshPro element dynamically and set the button text
            TextMeshProUGUI tmpText = levelBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                if (customFont != null)
                {
                    tmpText.font = customFont;
                }

                tmpText.color = Color.white;

                if (levelIndex <= unlockedLevel)
                {
                    tmpText.text = levelIndex.ToString();
                }
                else
                {
                    tmpText.color = Color.gray5;
                    tmpText.text = "?";
                }
            }
            else
            {
                Debug.LogWarning($"LevelLoader: Button prefab does not have a TextMeshProUGUI child component.");
            }

            // Automatically integrate it with your new SaveSystem logic!
            Button btnComponent = levelBtnObj.GetComponent<Button>();
            Image btnImage = levelBtnObj.GetComponent<Image>();

            if (btnComponent != null)
            {
                if (levelIndex <= unlockedLevel)
                {
                    btnComponent.interactable = true;
                    btnComponent.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
                    
                    if (btnImage != null && unlockedIcon != null)
                        btnImage.sprite = unlockedIcon;
                }
                else
                {
                    // Lock levels that the player hasn't reached yet
                    btnComponent.interactable = false;

                    if (btnImage != null && disabledIcon != null)
                        btnImage.sprite = disabledIcon;
                }
            }
        }
    }

    private void OnLevelButtonClicked(int levelIndex)
    {
        Debug.Log("Loading Level: " + levelIndex);
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("Level_" + levelIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level_" + levelIndex);
        }
    }
}
