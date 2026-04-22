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

    void Start()
    {
        initialCameraPosition = new Vector3(-4f, 0, 0);
    }

    void Update()
    {
        
    }
}
