using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RouteMapping
{
    [Tooltip("The item ID representing the block type (e.g., 0, 1, 2)")]
    public int itemType;
    [Tooltip("The conveyor belt the remaining items should take after consumption")]
    public ConveyorBelt nextBelt;
    [Tooltip("The light to turn on when this route is taken")]
    public Light routeLight;
}

public class Station : MonoBehaviour
{
    [Tooltip("Mappings defining which output belt to use based on the consumed item's type")]
    public RouteMapping[] routes;

    /// <summary>
    /// Processes the item type and returns the corresponding next conveyor belt.
    /// This method implicitly acts as the consumption step.
    /// </summary>
    /// <param name="itemType">The type of the leading block that hit the station</param>
    /// <returns>The connected ConveyorBelt for the remaining train, or null if no route exists/end of line.</returns>
    public virtual ConveyorBelt ConsumeAndRoute(int itemType)
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        float activeIntensity = levelManager != null ? levelManager.activeRouteLightIntensity : 5f;

        // Turn off all lights for this station first
        foreach (var route in routes)
        {
            if (route.routeLight != null)
            {
                route.routeLight.intensity = 0f;
            }
        }

        foreach (var route in routes)
        {
            if (route.itemType == itemType)
            {
                if (route.routeLight != null)
                {
                    route.routeLight.intensity = activeIntensity;
                }
                return route.nextBelt;
            }
        }
        
        Debug.LogWarning($"Station {gameObject.name} could not find a route mapping for item type: {itemType}. Train may stop.");
        return null;
    }

    /// <summary>
    /// Hook called by the QueueManager when the train becomes completely empty 
    /// after passing through this station.
    /// </summary>
    public virtual void OnAllBlocksProcessed()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
    }
}
