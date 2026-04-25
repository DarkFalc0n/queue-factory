using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This ensures the transition panel persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure the screen is clear at start if the canvas group is already assigned
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(Transition(buildIndex));
    }

    private IEnumerator Transition(string sceneName)
    {
        if (transitionCanvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup assigned to SceneTransitionManager. Loading scene instantly.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // 1. Fade to black
        transitionCanvasGroup.blocksRaycasts = true; // Prevent clicking during transition
        float timeElapsed = 0;
        
        while (timeElapsed < fadeDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transitionCanvasGroup.alpha = 1;

        // 2. Load the scene in the background
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 3. Fade to transparent again
        timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 0;
        transitionCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Transition(int buildIndex)
    {
        if (transitionCanvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup assigned to SceneTransitionManager. Loading scene instantly.");
            SceneManager.LoadScene(buildIndex);
            yield break;
        }

        // 1. Fade to black
        transitionCanvasGroup.blocksRaycasts = true; // Prevent clicking during transition
        float timeElapsed = 0;
        
        while (timeElapsed < fadeDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transitionCanvasGroup.alpha = 1;

        // 2. Load the scene in the background
        yield return SceneManager.LoadSceneAsync(buildIndex);

        // 3. Fade to transparent again
        timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        transitionCanvasGroup.alpha = 0;
        transitionCanvasGroup.blocksRaycasts = false;
    }
}
