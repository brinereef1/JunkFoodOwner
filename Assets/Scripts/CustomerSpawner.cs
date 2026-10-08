using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private CustomerWaypoint[] queueWaypoints;
    [SerializeField] private Transform[] exitWaypoints;
    [SerializeField] private float spawnInterval = 3f;

    private float spawnTimer;

    private void Start()
    {
        TrySpawnCustomer();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        // ---------------------------------------------
        // VALIDATION
        // ---------------------------------------------

        if (queueWaypoints == null ||
            queueWaypoints.Length == 0)
        {
            return;
        }

        if (exitWaypoints == null ||
            exitWaypoints.Length == 0)
        {
            return;
        }

        // ---------------------------------------------
        // FIRST EXIT PATH LAST ELEMENT
        // THIS IS THE MULTI-CUSTOMER WAITING AREA
        // ---------------------------------------------

        Transform lastExitWaypoint =
            exitWaypoints[exitWaypoints.Length - 1];

        CustomerWaitingArea waitingArea =
            lastExitWaypoint.GetComponent<CustomerWaitingArea>();

        if (waitingArea == null)
        {
            Debug.LogError(
                "Last exit waypoint must have a CustomerWaitingArea component!"
            );

            return;
        }

        // Stop spawning when all waiting slots are full.
        if (!waitingArea.HasFreeSlot)
        {
            Debug.Log(
                "Customer waiting area is full. Stop spawning."
            );

            return;
        }

        // ---------------------------------------------
        // FIRST QUEUE WAYPOINT
        // ---------------------------------------------
        //
        // Queue waypoints are SINGLE occupancy.
        //

        if (queueWaypoints[0].IsOccupied)
        {
            // First queue position is occupied.
            // Wait until the current customer moves forward.
            return;
        }

        // ---------------------------------------------
        // SPAWN CUSTOMER
        // ---------------------------------------------

    GameObject customerObject =
        PoolManager.Instance.Get(
            customerPrefab,
            queueWaypoints[0].transform.position,
            queueWaypoints[0].transform.rotation
        );

    CustomerMovement movement =
        customerObject.GetComponent<CustomerMovement>();

    if (movement == null)
    {
        Debug.LogError(
            "Customer prefab does not have CustomerMovement!"
        );

        PoolManager.Instance.Release(customerObject);
        return;
    }

    // Make sure the movement component is enabled.
    movement.enabled = true;

    // Convert CustomerWaypoint[] into Transform[]
    Transform[] queueTransforms =
        new Transform[queueWaypoints.Length];

    for (int i = 0; i < queueWaypoints.Length; i++)
    {
        queueTransforms[i] =
            queueWaypoints[i].transform;
    }

    movement.Initialize(
        queueTransforms,
        exitWaypoints
    );

    Debug.Log("Customer spawned.");
    }
}