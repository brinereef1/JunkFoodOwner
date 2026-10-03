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
        if (queueWaypoints == null ||
            queueWaypoints.Length == 0)
            return;

        // If spawn waypoint is occupied,
        // don't create another customer.
        if (!queueWaypoints[0].IsEmpty)
            return;

        GameObject customerObject =
            PoolManager.Instance.Get(
                customerPrefab,
                queueWaypoints[0].transform.position,
                queueWaypoints[0].transform.rotation
            );

        CustomerMovement movement =
            customerObject.GetComponent<CustomerMovement>();

        movement.Initialize(
            queueWaypoints,
            exitWaypoints
        );

        Debug.Log("Customer spawned.");
    }
}