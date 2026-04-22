using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Ordered waypoints mapping out the path of this conveyor section.")]
    public Transform[] waypoints;

    [Tooltip("The station located at the end of this conveyor belt, if any.")]
    public Station connectedStation;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawSphere(waypoints[i].position, 0.1f);
            }
        }
        
        if (waypoints.Length > 0 && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.1f);
        }
    }
}
