using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro namespace

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    [Tooltip("The time to wait between typing each letter.")]
    public float delayPerLetter = 0.05f;

    [Tooltip("The time to wait after typing a comma.")]
    public float commaDelay = 0.2f;

    [Tooltip("The time to wait after typing a period, exclamation, or question mark.")]
    public float periodDelay = 0.5f;

    [Tooltip("If true, the effect will start automatically when the object is enabled.")]
    public bool playOnEnable = true;

    [Tooltip("The full text to type out. If left empty, it will use the text currently in the component.")]
    [TextArea(3, 10)]
    public string fullText;

    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        
        // If fullText is empty, grab whatever text is already written in the inspector
        if (string.IsNullOrEmpty(fullText))
        {
            fullText = textComponent.text;
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartTyping();
        }
    }

    /// <summary>
    /// Starts the typing effect from the beginning.
    /// </summary>
    public void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextCoroutine(fullText));
    }

    /// <summary>
    /// Updates the text to be typed out and starts typing it.
    /// </summary>
    public void StartTyping(string newText)
    {
        fullText = newText;
        StartTyping();
    }

    /// <summary>
    /// Instantly finishes the typing effect and shows the full text.
    /// </summary>
    public void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        textComponent.text = fullText;
    }

    private IEnumerator TypeTextCoroutine(string textToType)
    {
        textComponent.text = "";
        
        foreach (char letter in textToType.ToCharArray())
        {
            textComponent.text += letter;
            
            float currentDelay = delayPerLetter;
            if (letter == ',')
            {
                currentDelay = commaDelay;
            }
            else if (letter == '.' || letter == '!' || letter == '?')
            {
                currentDelay = periodDelay;
            }

            yield return new WaitForSecondsRealtime(currentDelay); 
            // Using WaitForSecondsRealtime so it works even if Time.timeScale is 0 (e.g. paused)
        }

        typingCoroutine = null;
    }
}
