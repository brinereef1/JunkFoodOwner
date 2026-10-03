using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KitchenStation : MonoBehaviour
{
    [Header("Food")]
    [SerializeField] private FoodType foodType;
    [SerializeField] private GameObject foodPrefab;

    [Header("Pickup")]
    [SerializeField] private float pickupTime = 2f;

    [Header("Timer")]
    [SerializeField] private Image timerImage;

    private PlayerCarry currentPlayer;
    private Coroutine pickupCoroutine;

    private void Start()
    {
        if (timerImage != null)
        {
            timerImage.fillAmount = 0f;
            timerImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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

        // Show timer
        if (timerImage != null)
        {
            timerImage.fillAmount = 0f;
            timerImage.gameObject.SetActive(true);
        }

        float elapsedTime = 0f;

        while (elapsedTime < pickupTime)
        {
            // Player must still be here
            if (currentPlayer == null)
            {
                ResetTimer();
                yield break;
            }

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / pickupTime;

            if (timerImage != null)
            {
                timerImage.fillAmount = progress;
            }

            yield return null;
        }

        // Make sure the player is still there
        if (currentPlayer != null &&
            !currentPlayer.IsCarrying)
        {
            currentPlayer.PickUpFood(
                foodPrefab,
                foodType
            );

            Debug.Log(foodType + " is ready!");
        }

        ResetTimer();
        pickupCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
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

        ResetTimer();

        Debug.Log(
            "Player left " +
            foodType +
            " station before food was ready."
        );
    }

    private void ResetTimer()
    {
        if (timerImage == null)
            return;

        timerImage.fillAmount = 0f;
        timerImage.gameObject.SetActive(false);
    }
}