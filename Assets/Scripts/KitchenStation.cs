using System.Collections;
using UnityEngine;

public class KitchenStation : MonoBehaviour
{
    [SerializeField] private FoodType foodType;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float pickupTime = 2f;

    private PlayerCarry currentPlayer;
    private Coroutine pickupCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        // PlayerCarry is directly on CarryPoint.
        // Do NOT use GetComponentInParent here.
        PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (currentPlayer != null)
            return;

        if (playerCarry.IsCarrying)
            return;

        currentPlayer = playerCarry;

        pickupCoroutine = StartCoroutine(PickupFood());
    }

    private IEnumerator PickupFood()
    {
        Debug.Log("Preparing " + foodType + "...");

        yield return new WaitForSeconds(pickupTime);

        if (currentPlayer == null)
        {
            pickupCoroutine = null;
            yield break;
        }

        if (!currentPlayer.IsCarrying)
        {
            currentPlayer.PickUpFood(foodPrefab, foodType);
        }

        pickupCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        // Again, only react to CarryPoint itself.
        PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (playerCarry != currentPlayer)
            return;

        currentPlayer = null;

        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }

        Debug.Log("Player left kitchen before food was ready.");
    }
}