using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    
    [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("minSize")] 
    private float minY = 5f;
    
    [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("maxSize")] 
    private float maxY = 30f;

    private Vector3 targetPosition;
    private Camera cam;
    private QueueManager queueManager;

    void Start()
    {
        cam = GetComponent<Camera>();
        queueManager = FindObjectOfType<QueueManager>();

        if (levelManager != null)
        {
            targetPosition = new Vector3(levelManager.initialCameraPosition.x, transform.position.y, levelManager.initialCameraPosition.z);
        }
        else
        {
            targetPosition = transform.position;
        }

        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        transform.position = targetPosition;
    }

    void Update()
    {
        if (levelManager == null) return;

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null)
        {
            float scrollDelta = mouse.scroll.ReadValue().y;
            if (scrollDelta != 0f)
            {
                // A positive delta indicates scrolling up (zooming in), so we decrease Y
                targetPosition.y -= scrollDelta * zoomSpeed * 0.01f;
            }
            
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        if (queueManager != null && queueManager.currentState == QueueState.Running)
        {
            GameObject lastItem = queueManager.LastTrainCar;
            if (lastItem != null)
            {
                targetPosition.x = lastItem.transform.position.x;
                targetPosition.z = lastItem.transform.position.z;
            }
        }
        else
        {
            float horizontal = 0f;
            float vertical = 0f;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) horizontal += 1f;
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) horizontal -= 1f;
                if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) vertical += 1f;
                if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) vertical -= 1f;
            }

            Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
            
            targetPosition.x += movement.x * (moveSpeed * Time.deltaTime);
            targetPosition.z += movement.z * (moveSpeed * Time.deltaTime);
        }

        Vector3 origin = levelManager.origin;
        float halfWidth = levelManager.levelWidth / 2f;
        float halfHeight = levelManager.levelHeight / 2f;

        if (halfWidth > 0f && halfHeight > 0f)
        {
            float camHalfHeight = 0f;
            float camHalfWidth = 0f;

            if (cam != null)
            {
                // To prevent the camera from revealing out-of-bounds areas while zooming IN (because Lerp hasn't caught up to target Y), 
                // we calculate perspective bounds using whichever is higher: our physical height or our target height.
                float effectiveY = Mathf.Max(transform.position.y, targetPosition.y);
                float distanceToOriginY = effectiveY - origin.y;
                float distance = Mathf.Max(0f, distanceToOriginY); 
                
                camHalfHeight = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                camHalfWidth = camHalfHeight * cam.aspect;
            }

            // Using simple rectangular layout clamping as requested (no angular projection adjustment)
            float clampHalfWidth = Mathf.Max(0f, halfWidth - camHalfWidth);
            float clampHalfHeight = Mathf.Max(0f, halfHeight - camHalfHeight);

            float minX = origin.x - clampHalfWidth;
            float maxX = origin.x + clampHalfWidth;
            float minZ = origin.z - clampHalfHeight;
            float maxZ = origin.z + clampHalfHeight;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        }
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }
}
