using UnityEngine;

public class EmployeeMovement : MonoBehaviour
{
private enum EmployeeState
{
    Idle,
    GoingToFoodStation,
    WaitingForFood,
    GoingToDelivery,
    WaitingForDelivery
}

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Restaurant")]
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private KitchenStation[] foodStations;

    private CharacterController controller;
    private PlayerCarry playerCarry;

    private EmployeeState state = EmployeeState.Idle;

    private Customer targetCustomer;
    private KitchenStation targetStation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCarry = GetComponentInChildren<PlayerCarry>();
    }

    private void OnEnable()
    {
        state = EmployeeState.Idle;

        targetCustomer = null;
        targetStation = null;
    }

    private void Update()
    {
        switch (state)
        {
            case EmployeeState.Idle:
                FindCustomer();
                break;

            case EmployeeState.GoingToFoodStation:
                MoveToFoodStation();
                break;

            case EmployeeState.WaitingForFood:
                // Completely stopped.
                break;

            case EmployeeState.GoingToDelivery:
                MoveToDelivery();
                break;

            case EmployeeState.WaitingForDelivery:
                // Completely stopped.
                break;
        }
    }

    // ----------------------------------------------------
    // FIND CUSTOMER
    // ----------------------------------------------------

    private void FindCustomer()
    {
        if (playerCarry == null)
            return;

        if (playerCarry.IsCarrying)
            return;

        Customer[] customers =
            FindObjectsByType<Customer>(
                FindObjectsSortMode.None
            );

        foreach (Customer customer in customers)
        {
            if (customer.HasReceivedFood)
                continue;

            // Customer must currently be waiting
            // at the delivery point.
            if (!customer.IsInsideDeliveryPoint)
                continue;

            targetCustomer = customer;

            targetStation =
                FindFoodStation(
                    customer.RequestedFood
                );

            if (targetStation == null)
            {
                Debug.LogWarning(
                    "No food station found for " +
                    customer.RequestedFood
                );

                targetCustomer = null;
                return;
            }

            Debug.Log(
                "Employee taking order: " +
                customer.RequestedFood
            );

            state =
                EmployeeState.GoingToFoodStation;

            return;
        }
    }

    // ----------------------------------------------------
    // FIND FOOD STATION
    // ----------------------------------------------------

    private KitchenStation FindFoodStation(
        FoodType requestedFood)
    {
        foreach (KitchenStation station in foodStations)
        {
            if (station == null)
                continue;

            if (station.FoodType == requestedFood)
                return station;
        }

        return null;
    }

    // ----------------------------------------------------
    // MOVE TO FOOD STATION
    // ----------------------------------------------------

    private void MoveToFoodStation()
    {
        if (targetStation == null)
        {
            ResetJob();
            return;
        }

        MoveTowards(targetStation.transform);
    }

    // ----------------------------------------------------
    // FOOD STATION TRIGGER CALLED THIS
    // ----------------------------------------------------

    public void ReachedFoodStation(
        KitchenStation station)
    {
        if (state != EmployeeState.GoingToFoodStation)
            return;

        if (station != targetStation)
            return;

        Debug.Log(
            "Employee reached " +
            station.FoodType +
            " station. Waiting for food..."
        );

        // STOP MOVING
        state = EmployeeState.WaitingForFood;
    }

    // ----------------------------------------------------
    // FOOD READY CALLED BY KITCHEN
    // ----------------------------------------------------

    public void FoodReady()
    {
        if (state != EmployeeState.WaitingForFood)
            return;

        if (playerCarry == null)
            return;

        if (!playerCarry.IsCarrying)
            return;

        Debug.Log(
            "Employee picked up " +
            playerCarry.CarriedFoodType
        );

        state = EmployeeState.GoingToDelivery;
    }

    // ----------------------------------------------------
    // MOVE TO DELIVERY
    // ----------------------------------------------------

    private void MoveToDelivery()
    {
        if (deliveryPoint == null)
        {
            Debug.LogError(
                "Employee Delivery Point is not assigned!"
            );

            ResetJob();
            return;
        }

        MoveTowards(deliveryPoint);
    }

    // ----------------------------------------------------
    // DELIVERY TRIGGER CALLED THIS
    // ----------------------------------------------------

    public void ReachedDeliveryPoint()
    {
        if (state != EmployeeState.GoingToDelivery)
            return;

        Debug.Log(
            "Employee reached delivery point. " +
            "Waiting for delivery..."
        );

        // STOP COMPLETELY
        state = EmployeeState.WaitingForDelivery;
    }

    // ----------------------------------------------------
    // DELIVERY COMPLETED CALLED BY CUSTOMERDELIVERY
    // ----------------------------------------------------

    public void DeliveryCompleted()
    {
        if (state != EmployeeState.WaitingForDelivery)
            return;

        Debug.Log(
            "Employee completed delivery."
        );

        ResetJob();
    }

    // ----------------------------------------------------
    // MOVEMENT
    // ----------------------------------------------------

    private void MoveTowards(Transform target)
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        controller.Move(
            direction *
            moveSpeed *
            Time.deltaTime
        );

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    public void StartFoodPreparation(KitchenStation station)
    {
        if (state != EmployeeState.GoingToFoodStation)
            return;

        if (station != targetStation)
            return;

        Debug.Log(
            "Employee reached " +
            station.FoodType +
            " station. Preparing food..."
        );

        state = EmployeeState.WaitingForFood;
    }

    public void FoodPreparationCancelled()
    {
        if (state != EmployeeState.WaitingForFood)
            return;

        Debug.Log(
            "Employee left food station. Resuming movement."
        );

        state = EmployeeState.GoingToFoodStation;
    }
    
    private void ResetJob()
    {
        targetCustomer = null;
        targetStation = null;

        state = EmployeeState.Idle;
    }
}