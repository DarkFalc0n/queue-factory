using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public float levelWidth = 100f;
    
    public float levelHeight = 100f;
    
    public Vector3 initialCameraPosition;
    
    public Vector3 origin = Vector3.zero;

    public int queueLength = 3;

    public int[] allowedItems = {0, 1, 2};

    public Vector3 enqueuePosition;

    [Header("Conveyor Prefabs")]
    public GameObject straightPrefab;
    public GameObject cornerPrefab;

    [Header("Win Condition")]
    public Station targetStation;
    public GameObject winScreen;
    public GameObject loseScreen;

    void Start()
    {
        initialCameraPosition = new Vector3(-4f, 0, 0);
    }

    void Update()
    {
        
    }

    public void CheckWinCondition(Station consumedAt)
    {
        if (consumedAt == targetStation)
        {
            Debug.Log("win");
            if (winScreen != null) winScreen.SetActive(true);
        }
        else
        {
            Debug.Log("lose");
            ShowLoseScreen();
        }
    }

    public void ShowLoseScreen()
    {
        if (loseScreen != null) loseScreen.SetActive(true);
    }

    public void ReloadLevel()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void LoadMainMenu()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void OpenCreditsScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("Credits");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
        }
    }
}
