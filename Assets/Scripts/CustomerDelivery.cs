using UnityEngine;

public class CustomerDelivery : MonoBehaviour
{
    private PlayerCarry player;
    private Customer customer;

    private void OnTriggerEnter(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry != null)
        {
            player = playerCarry;
        }

        Customer customerComponent =
            other.GetComponentInParent<Customer>();

        if (customerComponent != null)
        {
            customer = customerComponent;

            customer.SetInsideDeliveryPoint(true);
        }

        TryDeliver();
    }

    private void OnTriggerStay(Collider other)
    {
        TryDeliver();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry != null &&
            player == playerCarry)
        {
            player = null;
        }

        Customer customerComponent =
            other.GetComponentInParent<Customer>();

        if (customerComponent != null)
        {
            customerComponent.SetInsideDeliveryPoint(false);

            if (customer == customerComponent)
            {
                customer = null;
            }
        }
    }

    private void TryDeliver()
    {
        if (player == null)
            return;

        if (customer == null)
            return;

        if (!customer.IsInsideDeliveryPoint)
            return;

        if (!player.IsCarrying)
            return;

        if (customer.HasReceivedFood)
            return;

        FoodType deliveredFood =
            player.CarriedFoodType;

        GameObject food =
            player.TakeFood();

        customer.ReceiveFood(
            food,
            deliveredFood
        );
    }
}