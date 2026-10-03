using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waypointDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 10f;

    private Customer customer;

    private CustomerWaypoint[] queueWaypoints;
    private Transform[] exitWaypoints;

    private int currentWaypointIndex;
    private int exitWaypointIndex;

    private bool isMoving;
    private bool isLeaving;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    public void Initialize(
        CustomerWaypoint[] queuePath,
        Transform[] exitPath)
    {
        queueWaypoints = queuePath;
        exitWaypoints = exitPath;

        currentWaypointIndex = 0;
        exitWaypointIndex = 0;

        isMoving = false;
        isLeaving = false;

        CustomerWaypoint startingWaypoint =
            queueWaypoints[0];

        startingWaypoint.Reserve(customer);
        customer.SetWaypoint(startingWaypoint);

        transform.SetPositionAndRotation(
            startingWaypoint.transform.position,
            startingWaypoint.transform.rotation
        );
    }

    private void Update()
    {
        if (isLeaving)
        {
            MoveToExit();
            return;
        }

        if (isMoving)
        {
            MoveToQueueWaypoint();
        }
        else
        {
            TryMoveToNextWaypoint();
        }
    }

    private void TryMoveToNextWaypoint()
    {
        if (currentWaypointIndex >= queueWaypoints.Length - 1)
            return;

        CustomerWaypoint nextWaypoint =
            queueWaypoints[currentWaypointIndex + 1];

        if (!nextWaypoint.IsEmpty)
            return;

        nextWaypoint.Reserve(customer);

        if (customer.CurrentWaypoint != null)
        {
            customer.CurrentWaypoint.Release(customer);
        }

        customer.SetWaypoint(nextWaypoint);

        currentWaypointIndex++;
        isMoving = true;
    }

    private void MoveToQueueWaypoint()
    {
        Transform target =
            queueWaypoints[currentWaypointIndex].transform;

        MoveTowards(target);

        if (ReachedTarget(target))
        {
            transform.position = target.position;
            isMoving = false;
        }
    }

    public void StartLeaving()
    {
        if (isLeaving)
            return;

        isLeaving = true;
        isMoving = false;

        if (customer.CurrentWaypoint != null)
        {
            customer.CurrentWaypoint.Release(customer);
            customer.SetWaypoint(null);
        }

        exitWaypointIndex = 0;

        if (exitWaypoints == null ||
            exitWaypoints.Length == 0)
        {
            ReturnToPool();
        }
    }

    private void MoveToExit()
    {
        if (exitWaypoints == null ||
            exitWaypoints.Length == 0)
            return;

        Transform target = exitWaypoints[exitWaypointIndex];

        MoveTowards(target);

        if (!ReachedTarget(target))
            return;

        transform.position = target.position;

        Debug.Log(
            gameObject.name +
            " reached Exit Waypoint " +
            exitWaypointIndex
        );

        // Are we at the final exit waypoint?
        if (exitWaypointIndex >= exitWaypoints.Length - 1)
        {
            ReturnToPool();
            return;
        }

        // Move to the next exit waypoint
        exitWaypointIndex++;
    }

    private void MoveTowards(Transform target)
    {
        Vector3 targetPosition = target.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private bool ReachedTarget(Transform target)
    {
        return Vector3.Distance(
            transform.position,
            target.position
        ) <= waypointDistance;
    }

    private void ReturnToPool()
    {
        Debug.Log(
            gameObject.name +
            " reached the end of the exit path."
        );

        customer.ReturnFoodToPool();

        PoolManager.Instance.Release(gameObject);
    }
}