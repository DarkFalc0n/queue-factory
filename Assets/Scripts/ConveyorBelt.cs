using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Ordered waypoints mapping out the path of this conveyor section.")]
    public Transform[] waypoints;

    [Tooltip("The station located at the end of this conveyor belt, if any.")]
    public Station connectedStation;

    [Header("Conveyor Settings")]
    public float cornerOffset = 0.5f;

    private void Start()
    {
        Transform meshContainer = transform.Find("GeneratedMeshes");
        if (meshContainer != null)
        {
            foreach (Transform child in meshContainer)
            {
                if (child.name.StartsWith("StraightSegment_"))
                {
                    ApplyTiling(child.gameObject, child.localScale.x);
                }
            }
        }
    }

    private void ApplyTiling(GameObject segment, float length)
    {
        Renderer[] renderers = segment.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Tiling"))
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                r.GetPropertyBlock(propBlock);
                
                // Grab the default tiling from the material
                Vector4 baseTiling = r.sharedMaterial.GetVector("_Tiling");
                
                // Multiply the length axis (Y component, because we swapped UVs in the shader) by the local length
                baseTiling.y *= length;
                
                propBlock.SetVector("_Tiling", baseTiling);
                r.SetPropertyBlock(propBlock);
            }
        }
    }

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

    [ContextMenu("Generate Belt meshes")]
    public void GenerateBeltMesh()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("Not enough waypoints to generate a belt.");
            return;
        }

        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("LevelManager not found in scene.");
            return;
        }

        GameObject straightPrefab = levelManager.straightPrefab;
        GameObject cornerPrefab = levelManager.cornerPrefab;

        if (straightPrefab == null || cornerPrefab == null)
        {
            Debug.LogError("Conveyor Prefabs are not assigned in LevelManager.");
            return;
        }

        // Safely find and delete the old meshes container without touching waypoints
        Transform oldContainer = transform.Find("GeneratedMeshes");
        if (oldContainer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(oldContainer.gameObject);
            }
            else
            {
                DestroyImmediate(oldContainer.gameObject);
            }
        }

        // Create a new container to hold the generated meshes
        GameObject meshContainer = new GameObject("GeneratedMeshes");
        meshContainer.transform.SetParent(transform, false);
        // SetParent with false ensures localPosition is zero, localRotation is identity, and localScale is one.

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;

            // Calculate everything in the parent's local space to properly respect the parent belt's transform (rotation, scale, tilt)
            Vector3 localStart = transform.InverseTransformPoint(waypoints[i].position);
            Vector3 localEnd = transform.InverseTransformPoint(waypoints[i + 1].position);
            Vector3 localDir = (localEnd - localStart).normalized;
            float localDist = Vector3.Distance(localStart, localEnd);

            bool isFirst = (i == 0);
            bool isLast = (i == waypoints.Length - 2);

            float startOff = isFirst ? 0f : cornerOffset;
            float endOff = isLast ? 0f : cornerOffset;

            float localLength = localDist - startOff - endOff;
            
            if (localLength > 0)
            {
                Vector3 localCenter = localStart + localDir * (startOff + localLength / 2f);
                
                GameObject straightObj = Instantiate(straightPrefab, meshContainer.transform);
                straightObj.name = "StraightSegment_" + i;
                straightObj.transform.localPosition = localCenter;
                
                // Align the local X axis with the path direction 'localDir'.
                // We use Vector3.up which is the local Up axis, allowing the belt to tilt with its parent.
                straightObj.transform.localRotation = Quaternion.LookRotation(Vector3.Cross(localDir, Vector3.up), Vector3.up);
                
                // Scale the path prefab to make it long along the local X axis
                Vector3 scale = straightObj.transform.localScale;
                scale.x = localLength;
                straightObj.transform.localScale = scale;

                // Adjust the material's tiling so the texture repeats instead of stretching
                ApplyTiling(straightObj, localLength);
            }

            // Generate corner
            if (!isLast && waypoints[i + 1] != null && waypoints[i + 2] != null)
            {
                Vector3 localNextEnd = transform.InverseTransformPoint(waypoints[i + 2].position);
                Vector3 localNextDir = (localNextEnd - localEnd).normalized;
                
                GameObject cornerObj = Instantiate(cornerPrefab, meshContainer.transform);
                cornerObj.name = "CornerSegment_" + i;
                cornerObj.transform.localPosition = localEnd;
                
                Vector3 cross = Vector3.Cross(localDir, localNextDir);
                bool isRightTurn = cross.y > 0;

                // To fit the corner properly for both left and right turns using the same prefab:
                if (isRightTurn)
                {
                    cornerObj.transform.localRotation = Quaternion.LookRotation(localDir, Vector3.up);
                }
                else
                {
                    cornerObj.transform.localRotation = Quaternion.LookRotation(localNextDir, Vector3.up);
                }
            }
        }
    }
}
