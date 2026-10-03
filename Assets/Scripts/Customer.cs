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
        movement = GetComponent<CustomerMovement>();
    }

    private void OnEnable()
    {
        HasReceivedFood = false;
        CurrentWaypoint = null;
        IsInsideDeliveryPoint = false;
        deliveredFood = null;

        CreateOrder();
    }

    private void CreateOrder()
    {
        int foodCount =
            System.Enum.GetValues(typeof(FoodType)).Length;

        RequestedFood =
            (FoodType)Random.Range(0, foodCount);

        Debug.Log(
            gameObject.name +
            " wants " +
            RequestedFood
        );

        // Show requested food above customer
        customerEmoji.ShowFoodRequest(RequestedFood);
    }

    public void SetWaypoint(CustomerWaypoint waypoint)
    {
        CurrentWaypoint = waypoint;
    }

    public void SetInsideDeliveryPoint(bool value)
    {
        IsInsideDeliveryPoint = value;
    }

    public void ReceiveFood(
        GameObject food,
        FoodType deliveredFoodType)
    {
        if (HasReceivedFood)
            return;

        HasReceivedFood = true;

        deliveredFood = food;

        // Move food to customer's FoodPoint
        if (deliveredFood != null)
        {
            deliveredFood.transform.SetParent(
                foodPoint,
                false
            );

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
            "Customer order: " + RequestedFood +
            " | Delivered: " + deliveredFoodType +
            " | Correct: " + correctOrder
        );

        // CHANGE EMOJI
        if (customerEmoji != null)
        {
            customerEmoji.ShowResult(correctOrder);
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                ": Customer Emoji reference is missing!"
            );
        }

        if (correctOrder)
        {
            int payment = FoodPricing.GetPrice(deliveredFoodType);

            MoneyManager.Instance.CreateCash(payment);

            Debug.Log(
                "Correct order! Customer generated $" +
                payment + " in cash."
            );
        }
        else
        {
            Debug.Log(
                "Wrong order! No money generated."
            );
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