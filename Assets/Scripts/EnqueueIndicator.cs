using UnityEngine;

public class EnqueueIndicator : MonoBehaviour
{
    [Tooltip("Reference to the QueueManager to monitor its state.")]
    [SerializeField] private QueueManager queueManager;

    [Header("Emission Pulse Settings")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float minEmissionIntensity = 0.5f;
    [SerializeField] private float maxEmissionIntensity = 3.0f;

    [Header("Emission Colors")]
    [ColorUsage(true, true)] // Enables HDR color picker in the inspector
    [SerializeField] private Color normalColor = Color.green;
    [ColorUsage(true, true)] 
    [SerializeField] private Color blockedColor = Color.red;

    private Renderer genericRenderer;
    private MaterialPropertyBlock propBlock;
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        genericRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        
        // Ensure the material has emission enabled if it's a standard shader
        if (genericRenderer != null && genericRenderer.material != null)
        {
            genericRenderer.material.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (queueManager == null || genericRenderer == null) return;
        bool isBlocked = queueManager.IsInstantiating || queueManager.IsQueueFull || queueManager.currentState != QueueState.Enqueuing;
        if (isBlocked)
        {
            // Blocked state: Constant Red Emission
            SetEmissionColor(blockedColor);
        }
        else
        {
            // Normal state: Pulsate Green Emission Intensity
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0 to 1
            float intensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, t);
            
            // Multiply the base color by the intensity for HDR emission
            Color currentEmission = normalColor * intensity;
            
            SetEmissionColor(currentEmission);
        }
    }

    private void SetEmissionColor(Color hdrColor)
    {
        // Using MaterialPropertyBlock prevents Unity from duplicating the material instance 
        // every time we change its color, which saves memory and maintains performance.
        genericRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColorProperty, hdrColor);
        genericRenderer.SetPropertyBlock(propBlock);
    }
}
