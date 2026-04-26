using UnityEngine;

public class Incinerator : Station
{
    [Header("Incinerator Settings")]
    [Tooltip("If true, logs a message whenever the entire line of blocks is completely destroyed.")]
    public bool logOnComplete = true;

    /// <summary>
    /// Incinerators don't route blocks anywhere; they just 'eat' them.
    /// Returning null naturally terminates the path for the block at this station.
    /// </summary>
    public override ConveyorBelt ConsumeAndRoute(int itemType)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.incineratorConsumeSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.incineratorConsumeSound);
        }

        // Custom logic to handle individual block incinerations can go here
        return null;
    }

    /// <summary>
    /// Triggered automatically by the QueueManager when the train becomes empty.
    /// </summary>
    public override void OnAllBlocksProcessed()
    {
        AllBlocksIncinerated();
    }

    /// <summary>
    /// Exposed function to signify all connected routing sequences have died in the incinerator.
    /// </summary>
    public void AllBlocksIncinerated()
    {
        if (logOnComplete)
        {
            Debug.Log("All blocks incinerated");
        }
        
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.ShowLoseScreen();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color to distinguish from regular stations
        Gizmos.DrawCube(transform.position, Vector3.one * 0.6f);
    }
}
