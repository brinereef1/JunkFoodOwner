using UnityEngine;

public class Customer : MonoBehaviour
{
    public bool HasReceivedFood { get; private set; }
    public FoodType RequestedFood { get; private set; }
    public CustomerWaypoint CurrentWaypoint { get; private set; }
    public bool IsInsideDeliveryPoint { get; private set; }

    [SerializeField] private Transform foodPoint;
    [SerializeField] private CustomerEmoji customerEmoji;

    private GameObject deliveredFood;
    private CustomerMovement movement;

    private void Awake()
    {
        // Store the movement script so we can start leaving after delivery.
        movement = GetComponent<CustomerMovement>();
    }

    private void OnEnable()
    {
        // Reset the order state each time this customer is reused from the pool.
        HasReceivedFood = false;
        CurrentWaypoint = null;
        IsInsideDeliveryPoint = false;
        deliveredFood = null;

        CreateOrder();
    }

    private void CreateOrder()
    {
        KitchenStation[] stations =
            FindObjectsByType<KitchenStation>(
                FindObjectsSortMode.None
            );

        System.Collections.Generic.List<FoodType>
            availableFood =
            new System.Collections.Generic.List<FoodType>();

        foreach (KitchenStation station in stations)
        {
            if (station == null)
                continue;

            if (!station.gameObject.activeInHierarchy)
                continue;

            if (!availableFood.Contains(
                    station.FoodType))
            {
                availableFood.Add(
                    station.FoodType
                );
            }
        }

        // Safety check
        if (availableFood.Count == 0)
        {
            Debug.LogWarning(
                "No active food stations available!"
            );

            return;
        }

        RequestedFood =
            availableFood[
                Random.Range(
                    0,
                    availableFood.Count
                )
            ];

        Debug.Log(
            gameObject.name +
            " wants " +
            RequestedFood
        );

        customerEmoji.ShowFoodRequest(
            RequestedFood
        );
    }

    public void SetWaypoint(CustomerWaypoint waypoint)
    {
        CurrentWaypoint = waypoint;
    }

    public void SetInsideDeliveryPoint(bool value)
    {
        IsInsideDeliveryPoint = value;
    }

    public void ReceiveFood(GameObject food, FoodType deliveredFoodType)
    {
        if (HasReceivedFood)
            return;

        HasReceivedFood = true;
        deliveredFood = food;

        // Put the delivered food at the customer handoff point.
        if (deliveredFood != null)
        {
            deliveredFood.transform.SetParent(foodPoint, false);
            deliveredFood.transform.localPosition = Vector3.zero;
            deliveredFood.transform.localRotation = Quaternion.identity;

            Rigidbody rb = deliveredFood.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        bool correctOrder = deliveredFoodType == RequestedFood;

        Debug.Log(
            "Customer order: " +
            RequestedFood +
            " | Delivered: " +
            deliveredFoodType +
            " | Correct: " +
            correctOrder
        );

        if (customerEmoji != null)
        {
            customerEmoji.ShowResult(correctOrder);
        }

        if (correctOrder)
        {
            int payment = FoodPricing.GetPrice(deliveredFoodType);
            MoneyManager.Instance.CreateCash(payment);

            Debug.Log(
                "Correct order! Customer generated $" +
                payment +
                " in cash."
            );
        }
        else
        {
            Debug.Log("Wrong order! No money generated.");
        }

        movement.StartLeaving();
    }

    public void ReturnFoodToPool()
    {
        if (deliveredFood == null)
            return;

        PoolManager.Instance.Release(deliveredFood);
        deliveredFood = null;
    }
}