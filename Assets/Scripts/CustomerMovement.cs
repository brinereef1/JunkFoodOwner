using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    private enum CustomerState
    {
        Idle,
        GoingToQueueWaypoint,
        WaitingForQueueWaypoint,
        GoingToExitWaypoint,
        WaitingForExitWaypoint,
        GoingToSeat,
        WaitingForSeat,
        Eating,
        GoingToRestaurantExit
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 0.15f;

    [Header("Eating")]
    [SerializeField] private float eatingTime = 2f;

    private NavMeshAgent agent;
    private Customer customer;

    private CustomerState currentState;

    // --------------------------------------------------
    // QUEUE
    // --------------------------------------------------

    private Transform[] queueWaypoints;
    private int currentQueueIndex = -1;

    // --------------------------------------------------
    // FIRST EXIT PATH
    // --------------------------------------------------

    private Transform[] exitWaypoints;
    private int currentExitIndex = -1;

    private CustomerWaitingArea exitWaitingArea;

    // --------------------------------------------------
    // TABLE / SEATS
    // --------------------------------------------------

    private Transform reservedSeat;
    private Transform reservedTable;

    private static readonly Dictionary<Transform, CustomerMovement> seatOwners =
        new Dictionary<Transform, CustomerMovement>();

    // --------------------------------------------------
    // WAITING AREA
    // --------------------------------------------------

    private int reservedWaitingSlot = -1;

    // --------------------------------------------------
    // FINAL RESTAURANT EXIT
    // --------------------------------------------------

    private Transform[] restaurantExitWaypoints;
    private int currentRestaurantExitIndex = -1;

    // --------------------------------------------------
    // SETUP
    // --------------------------------------------------

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        customer = GetComponent<Customer>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        FindFinalExit();
    }

    private void OnEnable()
    {
        currentState = CustomerState.Idle;

        currentQueueIndex = -1;
        currentExitIndex = -1;
        currentRestaurantExitIndex = -1;

        reservedSeat = null;
        reservedWaitingSlot = -1;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    private void OnDisable()
    {
        ReleaseCurrentSeat();
        ReleaseCurrentWaitingSlot();
        ReleaseCurrentQueueWaypoint();
        ReleaseCurrentExitWaypoint();
        ReleaseCurrentRestaurantExitWaypoint();

        StopAllCoroutines();
    }

    // ==================================================
    // INITIALIZE
    // ==================================================

    public void Initialize(Transform[] queuePath, Transform[] exitPath)
    {
        queueWaypoints = queuePath;
        exitWaypoints = exitPath;

        currentQueueIndex = -1;
        currentExitIndex = -1;

        ReleaseCurrentQueueWaypoint();
        ReleaseCurrentExitWaypoint();

        // ----------------------------------------------
        // Validate queue
        // ----------------------------------------------

        if (queueWaypoints == null || queueWaypoints.Length == 0)
        {
            Debug.LogError("Customer has no queue waypoints.");
            return;
        }

        // ----------------------------------------------
        // Get waiting area from LAST EXIT WAYPOINT
        // ----------------------------------------------

        exitWaitingArea = null;

        if (exitWaypoints != null && exitWaypoints.Length > 0)
        {
            Transform lastExitPoint = exitWaypoints[exitWaypoints.Length - 1];

            exitWaitingArea =
                lastExitPoint.GetComponent<CustomerWaitingArea>();

            if (exitWaitingArea == null)
            {
                Debug.LogError(
                    "Last exit waypoint must have a CustomerWaitingArea component."
                );
            }
        }

        // ----------------------------------------------
        // Reserve FIRST queue waypoint
        // ----------------------------------------------

        CustomerWaypoint firstWaypoint =
            queueWaypoints[0].GetComponent<CustomerWaypoint>();

        if (firstWaypoint == null)
        {
            Debug.LogError(
                queueWaypoints[0].name +
                " needs a CustomerWaypoint component."
            );

            return;
        }

        if (!firstWaypoint.TryReserve(customer))
        {
            Debug.LogWarning(
                "Could not reserve first queue waypoint for customer."
            );

            return;
        }

        currentQueueIndex = 0;

        MoveTo(queueWaypoints[0]);

        currentState = CustomerState.GoingToQueueWaypoint;
    }

    // ==================================================
    // UPDATE
    // ==================================================

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.GoingToQueueWaypoint:
                UpdateQueueMovement();
                break;

            case CustomerState.WaitingForQueueWaypoint:
                TryMoveToNextQueueWaypoint();
                break;

            case CustomerState.GoingToExitWaypoint:
                UpdateExitMovement();
                break;

            case CustomerState.WaitingForExitWaypoint:
                TryContinueExitPath();
                break;

            case CustomerState.GoingToSeat:
                UpdateSeatMovement();
                break;

            case CustomerState.WaitingForSeat:
                TryGetSeatFromWaitingArea();
                break;

            case CustomerState.GoingToRestaurantExit:
                UpdateRestaurantExitMovement();
                break;
        }
    }

    // ==================================================
    // QUEUE MOVEMENT
    // ==================================================

    private void UpdateQueueMovement()
    {
        if (!HasReachedDestination())
            return;

        StopAgent();

        // We reached the current queue waypoint.
        // Do NOT automatically leave it until the next
        // waypoint is available.
        TryMoveToNextQueueWaypoint();
    }

    private void TryMoveToNextQueueWaypoint()
    {
        if (queueWaypoints == null)
            return;

        // Already at the final queue waypoint.
        // Wait here for the customer to receive food.
        if (currentQueueIndex >= queueWaypoints.Length - 1)
        {
            currentState = CustomerState.Idle;
            StopAgent();
            return;
        }

        int nextIndex = currentQueueIndex + 1;

        CustomerWaypoint nextWaypoint =
            queueWaypoints[nextIndex].GetComponent<CustomerWaypoint>();

        if (nextWaypoint == null)
        {
            Debug.LogError(
                queueWaypoints[nextIndex].name +
                " needs a CustomerWaypoint component."
            );

            return;
        }

        // ------------------------------------------------
        // NEXT QUEUE POSITION IS OCCUPIED
        // ------------------------------------------------
        //
        // IMPORTANT:
        // Do NOT go Idle permanently.
        // Keep checking until it becomes free.
        //

        if (!nextWaypoint.TryReserve(customer))
        {
            currentState =
                CustomerState.WaitingForQueueWaypoint;

            StopAgent();

            return;
        }

        // ------------------------------------------------
        // NEXT POSITION IS AVAILABLE
        // ------------------------------------------------

        ReleaseCurrentQueueWaypoint();

        currentQueueIndex = nextIndex;

        MoveTo(queueWaypoints[nextIndex]);

        currentState =
            CustomerState.GoingToQueueWaypoint;
    }
    // ==================================================
    // CALLED AFTER CUSTOMER RECEIVES FOOD
    // ==================================================

    public void StartLeaving()
    {
        StopAllCoroutines();

        // Customer no longer needs the delivery queue position.
        ReleaseCurrentQueueWaypoint();

        currentQueueIndex = -1;

        // No exit path configured.
        if (exitWaypoints == null || exitWaypoints.Length == 0)
        {
            Debug.LogError("Customer has no exit waypoints.");
            TryReserveSeatOrWait();
            return;
        }

        currentExitIndex = -1;

        TryMoveToNextExitWaypoint();
    }

    // ==================================================
    // FIRST EXIT PATH
    // ==================================================

    private void TryMoveToNextExitWaypoint()
    {
        if (exitWaypoints == null || exitWaypoints.Length == 0)
        {
            TryReserveSeatOrWait();
            return;
        }

        int nextIndex = currentExitIndex + 1;

        if (nextIndex >= exitWaypoints.Length)
        {
            TryReserveSeatOrWait();
            return;
        }

        bool isLastExitWaypoint =
            nextIndex == exitWaypoints.Length - 1;

        Transform nextPoint = exitWaypoints[nextIndex];

        // ------------------------------------------------
        // NORMAL EXIT WAYPOINT
        // ------------------------------------------------

        if (!isLastExitWaypoint)
        {
            CustomerWaypoint waypoint =
                nextPoint.GetComponent<CustomerWaypoint>();

            if (waypoint == null)
            {
                Debug.LogError(
                    nextPoint.name +
                    " needs a CustomerWaypoint component."
                );

                return;
            }

            // First exit path waypoints are also SINGLE occupancy.
            if (!waypoint.TryReserve(customer))
            {
                currentState = CustomerState.WaitingForExitWaypoint;
                return;
            }

            ReleaseCurrentExitWaypoint();

            currentExitIndex = nextIndex;

            MoveTo(nextPoint);

            currentState = CustomerState.GoingToExitWaypoint;

            return;
        }

        // ------------------------------------------------
        // LAST EXIT WAYPOINT
        // ------------------------------------------------
        //
        // This one is different.
        // It is the multi-customer waiting area.
        //

        if (exitWaitingArea == null)
        {
            Debug.LogError(
                "Last exit waypoint has no CustomerWaitingArea."
            );

            return;
        }

        // If a seat is available, we don't need a waiting slot.
        bool seatAvailable = HasFreeSeat();

        if (!seatAvailable && !exitWaitingArea.HasFreeSlot)
        {
            // Waiting area completely full.
            // Stay where we are instead of allowing customers
            // to stack on top of each other.
            currentState = CustomerState.WaitingForExitWaypoint;
            return;
        }

        ReleaseCurrentExitWaypoint();

        currentExitIndex = nextIndex;

        MoveTo(nextPoint);

        currentState = CustomerState.GoingToExitWaypoint;
    }

    private void UpdateExitMovement()
    {
        if (!HasReachedDestination())
            return;

        StopAgent();

        bool isLastExitWaypoint =
            currentExitIndex == exitWaypoints.Length - 1;

        if (!isLastExitWaypoint)
        {
            // Continue to the next exit waypoint.
            TryMoveToNextExitWaypoint();
            return;
        }

        // We have reached the waiting area.
        TryReserveSeatOrWait();
    }

    private void TryContinueExitPath()
    {
        TryMoveToNextExitWaypoint();
    }

    // ==================================================
    // SEAT / WAITING AREA
    // ==================================================

    private void TryReserveSeatOrWait()
    {
        // First priority:
        // Find an available restaurant seat.
        if (TryReserveSeat())
        {
            MoveTo(reservedSeat);

            currentState = CustomerState.GoingToSeat;

            return;
        }

        // No seat available.
        // Reserve a waiting slot.
        if (exitWaitingArea != null)
        {
            if (exitWaitingArea.TryReserve(customer))
            {
                reservedWaitingSlot =
                    exitWaitingArea.GetReservedSlotIndex(customer);

                Transform waitingTarget =
                    exitWaitingArea.GetTargetTransform(customer);

                MoveTo(waitingTarget);

                currentState = CustomerState.WaitingForSeat;

                return;
            }

            // No waiting slots either.
            // Stay here until something becomes available.
            currentState = CustomerState.WaitingForSeat;
            StopAgent();

            return;
        }

        Debug.LogError(
            "No seat available and CustomerWaitingArea is missing."
        );
    }

    // ==================================================
    // WAITING CUSTOMER TRIES AGAIN
    // ==================================================

    private void TryGetSeatFromWaitingArea()
    {
        // Don't do anything until a seat becomes free.
        if (!HasFreeSeat())
            return;

        if (!TryReserveSeat())
            return;

        // We successfully got a seat.
        ReleaseCurrentWaitingSlot();

        MoveTo(reservedSeat);

        currentState = CustomerState.GoingToSeat;
    }

    // ==================================================
    // SEAT MOVEMENT
    // ==================================================

    private void UpdateSeatMovement()
    {
        if (!HasReachedDestination())
            return;

        StopAgent();

        // Snap exactly onto the seat.
        transform.position = reservedSeat.position;

        // Face the table.
        if (reservedTable != null)
        {
            Vector3 direction =
                reservedTable.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }

        currentState = CustomerState.Eating;

        StartCoroutine(EatingRoutine());
    }

    private IEnumerator EatingRoutine()
    {
        yield return new WaitForSeconds(eatingTime);

        // Eating is finished.
        // Remove the food from the table and return it to the pool.
        customer.ReturnFoodToPool();

        // Free the seat.
        ReleaseCurrentSeat();

        // Now leave the restaurant.
        StartRestaurantExit();
    }

    // ==================================================
    // SEAT SYSTEM
    // ==================================================

    private bool HasFreeSeat()
    {
        if (TableManager.Instance == null)
            return false;

        foreach (Transform table in TableManager.Instance.Tables)
        {
            if (table == null)
                continue;

            // Ignore locked tables.
            if (!table.gameObject.activeInHierarchy)
                continue;

            for (int i = 0; i < table.childCount; i++)
            {
                Transform seat = table.GetChild(i);

                if (!seatOwners.ContainsKey(seat))
                    return true;
            }
        }

        return false;
    }

    private bool TryReserveSeat()
    {
        if (reservedSeat != null)
            return true;

        if (TableManager.Instance == null)
            return false;

        foreach (Transform table in TableManager.Instance.Tables)
        {
            if (table == null)
                continue;

            // Locked table.
            if (!table.gameObject.activeInHierarchy)
                continue;

            for (int i = 0; i < table.childCount; i++)
            {
                Transform seat = table.GetChild(i);

                if (seatOwners.ContainsKey(seat))
                    continue;

                // Reserve this seat.
                seatOwners.Add(seat, this);

                reservedSeat = seat;
                reservedTable = table;

                Debug.Log(
                    name +
                    " reserved " +
                    seat.name +
                    " at " +
                    table.name
                );

                return true;
            }
        }

        return false;
    }

    private void ReleaseCurrentSeat()
    {
        if (reservedSeat == null)
            return;

        if (seatOwners.TryGetValue(
            reservedSeat,
            out CustomerMovement owner))
        {
            if (owner == this)
                seatOwners.Remove(reservedSeat);
        }

        reservedSeat = null;
        reservedTable = null;
    }

    // ==================================================
    // FINAL RESTAURANT EXIT
    // ==================================================

    private void FindFinalExit()
    {
        GameObject finalExitObject = null;

        try
        {
            finalExitObject =
                GameObject.FindGameObjectWithTag("FinalExitPoint");
        }
        catch
        {
            Debug.LogError(
                "FinalExitPoint tag does not exist."
            );

            return;
        }

        if (finalExitObject == null)
        {
            Debug.LogError(
                "No GameObject with FinalExitPoint tag was found."
            );

            return;
        }

        Transform exitParent = finalExitObject.transform;

        int childCount = exitParent.childCount;

        if (childCount == 0)
        {
            Debug.LogError(
                "FinalExitPoint has no child waypoints."
            );

            return;
        }

        restaurantExitWaypoints =
            new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            restaurantExitWaypoints[i] =
                exitParent.GetChild(i);
        }
    }

    private void StartRestaurantExit()
    {
        if (restaurantExitWaypoints == null ||
            restaurantExitWaypoints.Length == 0)
        {
            ReturnCustomerToPool();
            return;
        }

        currentRestaurantExitIndex = -1;

        TryMoveToNextRestaurantExitWaypoint();
    }

    private void TryMoveToNextRestaurantExitWaypoint()
    {
        int nextIndex =
            currentRestaurantExitIndex + 1;

        if (nextIndex >= restaurantExitWaypoints.Length)
        {
            ReturnCustomerToPool();
            return;
        }

        Transform nextPoint =
            restaurantExitWaypoints[nextIndex];

        CustomerWaypoint waypoint =
            nextPoint.GetComponent<CustomerWaypoint>();

        if (waypoint == null)
        {
            Debug.LogError(
                nextPoint.name +
                " needs a CustomerWaypoint component."
            );

            return;
        }

        // Final exit waypoints are single occupancy.
        if (!waypoint.TryReserve(customer))
        {
            currentState =
                CustomerState.GoingToRestaurantExit;

            return;
        }

        ReleaseCurrentRestaurantExitWaypoint();

        currentRestaurantExitIndex = nextIndex;

        MoveTo(nextPoint);

        currentState =
            CustomerState.GoingToRestaurantExit;
    }

    private void UpdateRestaurantExitMovement()
    {
        if (!HasReachedDestination())
            return;

        StopAgent();

        if (currentRestaurantExitIndex ==
            restaurantExitWaypoints.Length - 1)
        {
            ReleaseCurrentRestaurantExitWaypoint();

            ReturnCustomerToPool();

            return;
        }

        TryMoveToNextRestaurantExitWaypoint();
    }

    // ==================================================
    // WAYPOINT RELEASE
    // ==================================================

    private void ReleaseCurrentQueueWaypoint()
    {
        if (queueWaypoints == null)
            return;

        if (currentQueueIndex < 0 ||
            currentQueueIndex >= queueWaypoints.Length)
            return;

        CustomerWaypoint waypoint =
            queueWaypoints[currentQueueIndex]
                .GetComponent<CustomerWaypoint>();

        if (waypoint != null)
            waypoint.Release(customer);

        currentQueueIndex = -1;
    }

    private void ReleaseCurrentExitWaypoint()
    {
        if (exitWaypoints == null)
            return;

        if (currentExitIndex < 0 ||
            currentExitIndex >= exitWaypoints.Length)
            return;

        // Last exit waypoint is NOT a CustomerWaypoint.
        if (currentExitIndex ==
            exitWaypoints.Length - 1)
            return;

        CustomerWaypoint waypoint =
            exitWaypoints[currentExitIndex]
                .GetComponent<CustomerWaypoint>();

        if (waypoint != null)
            waypoint.Release(customer);
    }

    private void ReleaseCurrentRestaurantExitWaypoint()
    {
        if (restaurantExitWaypoints == null)
            return;

        if (currentRestaurantExitIndex < 0 ||
            currentRestaurantExitIndex >=
            restaurantExitWaypoints.Length)
            return;

        CustomerWaypoint waypoint =
            restaurantExitWaypoints[
                currentRestaurantExitIndex
            ].GetComponent<CustomerWaypoint>();

        if (waypoint != null)
            waypoint.Release(customer);

        currentRestaurantExitIndex = -1;
    }

    private void ReleaseCurrentWaitingSlot()
    {
        if (exitWaitingArea == null)
            return;

        if (reservedWaitingSlot == -1)
            return;

        exitWaitingArea.Release(customer);

        reservedWaitingSlot = -1;
    }

    // ==================================================
    // NAVIGATION
    // ==================================================

    private void MoveTo(Transform target)
    {
        if (target == null)
            return;

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                name + " is not currently on the NavMesh."
            );

            return;
        }

        agent.isStopped = false;

        agent.SetDestination(target.position);
    }

    private void StopAgent()
    {
        if (agent == null)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private bool HasReachedDestination()
    {
        if (agent == null)
            return false;

        if (!agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance >
            agent.stoppingDistance + 0.05f)
            return false;

        if (agent.hasPath &&
            agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }

    // ==================================================
    // POOL
    // ==================================================

    private void ReturnCustomerToPool()
    {
        ReleaseCurrentSeat();
        ReleaseCurrentWaitingSlot();
        ReleaseCurrentQueueWaypoint();
        ReleaseCurrentExitWaypoint();
        ReleaseCurrentRestaurantExitWaypoint();

        currentState = CustomerState.Idle;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}