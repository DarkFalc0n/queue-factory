using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum QueueState
{
    Enqueuing,
    Running,
    Completed
}

public class QueueManager : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameData gameData;
    [SerializeField] private float moveDelay;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveDistance;

    [Header("Conveyor Settings")]
    [Tooltip("The initial conveyor belt the items will transfer to when Running.")]
    public ConveyorBelt startBelt;

    public QueueState currentState = QueueState.Enqueuing;

    private int enqueueIndex;
    private bool isInstantiating = false;
    private Queue<int> queue;
    private GameObject[] queueItems;

    public bool IsInstantiating => isInstantiating;
    public bool IsQueueFull => queue != null && levelManager != null && queue.Count >= levelManager.queueLength;

    private readonly Key[] topRowKeys = { Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };
    private readonly Key[] numpadKeys = { Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9 };

    void Start()
    {
        enqueueIndex = 0;
        queue = new Queue<int>();
        queueItems = new GameObject[levelManager.queueLength + 1];
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (currentState == QueueState.Enqueuing)
        {
            if (Keyboard.current[Key.Enter].wasPressedThisFrame || Keyboard.current[Key.NumpadEnter].wasPressedThisFrame)
            {
                if (queue.Count >= levelManager.queueLength && !isInstantiating)
                {
                    StartCoroutine(RunConveyorTrain());
                }
            }

            if (Keyboard.current[Key.Backspace].wasPressedThisFrame)
            {
                RemoveLastElement();
            }

            for (int i = 0; i <= 9; i++)
            {
                if (Keyboard.current[topRowKeys[i]].wasPressedThisFrame || Keyboard.current[numpadKeys[i]].wasPressedThisFrame)
                {
                    if (System.Array.IndexOf(levelManager.allowedItems, i) != -1)
                    {
                        Enqueue(i);
                    }
                }
            }
        }
    }

    public void Enqueue(int item)
    {
        if (queue.Count >= levelManager.queueLength)
        {
            Debug.LogWarning("Attempted to enqueue into a full queue!");
            return;
        }

        if (isInstantiating)
        {
            Debug.LogWarning("Queue is busy instantiating and moving. Please wait.");
            return;
        }

        queue.Enqueue(item);
        StartCoroutine(HandleEnqueue(item));
    }

    private IEnumerator HandleEnqueue(int item)
    {
        isInstantiating = true;

        GameObject instantiatedItem = Instantiate(gameData.queueItemPrefabs[item], levelManager.enqueuePosition, Quaternion.identity);
        queueItems[enqueueIndex] = instantiatedItem;
        enqueueIndex++;

        yield return new WaitForSeconds(moveDelay);

        float distanceCovered = 0f;
        Vector3[] targetPositions = new Vector3[enqueueIndex];

        for (int i = 0; i < enqueueIndex; i++)
        {
            targetPositions[i] = queueItems[i].transform.position + Vector3.right * moveDistance;
        }

        while (distanceCovered < moveDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            
            if (distanceCovered + step > moveDistance)
            {
                step = moveDistance - distanceCovered;
            }

            distanceCovered += step;

            for (int i = 0; i < enqueueIndex; i++)
            {
                if (queueItems[i] != null)
                {
                    queueItems[i].transform.Translate(Vector3.right * step, Space.World);
                }
            }

            yield return null;
        }

        for (int i = 0; i < enqueueIndex; i++)
        {
            if (queueItems[i] != null)
            {
                queueItems[i].transform.position = targetPositions[i];
            }
        }
        
        isInstantiating = false; 
    }

    public void RemoveLastElement()
    {
        if (isInstantiating)
        {
            Debug.LogWarning("Queue is busy. Please wait.");
            return;
        }

        if (queue.Count == 0)
        {
            Debug.LogWarning("Queue is empty, nothing to remove!");
            return;
        }

        StartCoroutine(HandleRemoveLast());
    }

    private IEnumerator HandleRemoveLast()
    {
        isInstantiating = true;

        int[] queueArray = queue.ToArray();
        queue.Clear();
        for (int i = 0; i < queueArray.Length - 1; i++)
        {
            queue.Enqueue(queueArray[i]);
        }

        enqueueIndex--;
        Destroy(queueItems[enqueueIndex]);
        queueItems[enqueueIndex] = null;

        float distanceCovered = 0f;
        Vector3[] targetPositions = new Vector3[enqueueIndex];

        for (int i = 0; i < enqueueIndex; i++)
        {
            if (queueItems[i] != null)
            {
                targetPositions[i] = queueItems[i].transform.position - Vector3.right * moveDistance;
            }
        }

        while (distanceCovered < moveDistance)
        {
            float step = moveSpeed * Time.deltaTime;

            if (distanceCovered + step > moveDistance)
            {
                step = moveDistance - distanceCovered;
            }

            distanceCovered += step;

            for (int i = 0; i < enqueueIndex; i++)
            {
                if (queueItems[i] != null)
                {
                    queueItems[i].transform.Translate(Vector3.left * step, Space.World);
                }
            }

            yield return null;
        }

        for (int i = 0; i < enqueueIndex; i++)
        {
            if (queueItems[i] != null)
            {
                queueItems[i].transform.position = targetPositions[i];
            }
        }

        isInstantiating = false;
    }

    public int Dequeue()
    {
        if (queue.Count > 0)
        {
            return queue.Dequeue();
        }
        
        Debug.LogWarning("Attempted to dequeue from an empty queue!");
        return default;
    }

    // --- CONVEYOR TRAIN LOGIC ---

    private IEnumerator RunConveyorTrain()
    {
        currentState = QueueState.Running;

        // --- Spawn Treasure Block ---
        yield return new WaitForSeconds(moveDelay);
        GameObject treasureBlock = Instantiate(gameData.queueItemPrefabs[0], levelManager.enqueuePosition, Quaternion.identity);
        queueItems[enqueueIndex] = treasureBlock;
        enqueueIndex++;
        // -----------------------------

        if (startBelt == null)
        {
            Debug.LogError("StartBelt is not assigned in QueueManager!");
            currentState = QueueState.Enqueuing;
            yield break;
        }

        List<GameObject> trainCars = new List<GameObject>();
        List<int> trainTypes = new List<int>();

        int[] queueArray = queue.ToArray();
        for (int i = 0; i < queueArray.Length; i++)
        {
            trainCars.Add(queueItems[i]);
            trainTypes.Add(queueArray[i]);
        }
        trainCars.Add(queueItems[queueArray.Length]);
        trainTypes.Add(0);

        List<Vector3> pathNodes = new List<Vector3>();
        foreach (var node in startBelt.waypoints)
        {
            if (node != null) pathNodes.Add(node.position);
        }

        if (pathNodes.Count == 0)
        {
            Debug.LogError("StartBelt has no waypoints!");
            currentState = QueueState.Enqueuing;
            yield break;
        }

        float[] carDistances = new float[trainCars.Count];
        for (int i = 0; i < trainCars.Count; i++)
        {
            carDistances[i] = -Vector3.Distance(trainCars[i].transform.position, pathNodes[0]);
        }

        ConveyorBelt currentBelt = startBelt;
        Station lastStation = null;

        while (trainCars.Count > 0)
        {
            float step = moveSpeed * Time.deltaTime;

            for (int i = 0; i < trainCars.Count; i++)
            {
                carDistances[i] += step;

                if (carDistances[i] >= 0)
                {
                    MoveCarToPathDistance(trainCars[i], carDistances[i], pathNodes);
                }
                else
                {
                    trainCars[i].transform.position = Vector3.MoveTowards(trainCars[i].transform.position, pathNodes[0], step);
                }
            }

            while (carDistances.Length > 0 && carDistances[0] >= GetPathLength(pathNodes))
            {
                if (currentBelt != null && currentBelt.connectedStation != null)
                {
                    lastStation = currentBelt.connectedStation;
                    int consumedType = trainTypes[0];
                    ConveyorBelt nextBelt = currentBelt.connectedStation.ConsumeAndRoute(consumedType);

                    if (trainCars.Count == 1)
                    {
                        levelManager.CheckWinCondition(lastStation);
                    }

                    if (nextBelt != null && nextBelt.waypoints.Length > 0)
                    {
                        currentBelt = nextBelt;
                        for (int n = 1; n < nextBelt.waypoints.Length; n++)
                        {
                            if (nextBelt.waypoints[n] != null)
                                pathNodes.Add(nextBelt.waypoints[n].position);
                        }
                    }
                    else
                    {
                        currentBelt = null;
                    }
                }
                else
                {
                    currentBelt = null;
                }

                if (trainCars[0] != null)
                {
                    Destroy(trainCars[0]);
                }

                trainCars.RemoveAt(0);
                trainTypes.RemoveAt(0);

                if (trainCars.Count == 0) break;

                float[] nextDists = new float[trainCars.Count];
                for (int d = 0; d < trainCars.Count; d++)
                {
                    nextDists[d] = carDistances[d + 1];
                }
                carDistances = nextDists;
            }

            yield return null;
        }

        if (lastStation != null)
        {
            lastStation.OnAllBlocksProcessed();
        }

        queue.Clear();
        enqueueIndex = 0;
        System.Array.Clear(queueItems, 0, queueItems.Length);
        currentState = QueueState.Completed;
    }

    private void MoveCarToPathDistance(GameObject car, float dist, List<Vector3> nodes)
    {
        float accumulated = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float segLen = Vector3.Distance(nodes[i], nodes[i+1]);
            if (dist <= accumulated + segLen)
            {
                float t = (dist - accumulated) / segLen;
                car.transform.position = Vector3.Lerp(nodes[i], nodes[i+1], t);
                return;
            }
            accumulated += segLen;
        }
        car.transform.position = nodes[nodes.Count - 1]; // Snap if over
    }

    private float GetPathLength(List<Vector3> nodes)
    {
        float len = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            len += Vector3.Distance(nodes[i], nodes[i+1]);
        }
        return len;
    }
}
