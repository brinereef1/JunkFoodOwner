using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KitchenStation : MonoBehaviour
{
    [SerializeField] private FoodType foodType;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float pickupTime = 2f;

    [Header("Loading")]
    [SerializeField] private GameObject loadingImageObject;

    public FoodType FoodType => foodType;

    private PlayerCarry currentPlayer;
    private Coroutine pickupCoroutine;

    private Image loadingImage;

    private void Awake()
    {
        // Link the loading image so we can show progress while food is being made.
        if (loadingImageObject != null)
        {
            loadingImage =
                loadingImageObject.GetComponent<Image>();

            if (loadingImage == null)
            {
                Debug.LogError(
                    gameObject.name +
                    ": Loading Image GameObject does not have an Image component!"
                );
            }
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                ": Loading Image Object is not assigned!"
            );
        }
    }

    private void Start()
    {
        // Start hidden until the player begins making food.
        DisableLoadingImage();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only a player can start food prep here.
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (currentPlayer != null)
            return;

        if (playerCarry.IsCarrying)
            return;

        currentPlayer = playerCarry;

        EmployeeMovement employee =
            playerCarry.GetComponentInParent<EmployeeMovement>();

        if (employee != null)
        {
            employee.StartFoodPreparation(this);
        }

        StartLoadingImage();

        pickupCoroutine =
            StartCoroutine(PickupFood());
    }

    private IEnumerator PickupFood()
    {
        // The player is making food at this station.
        Debug.Log(
            foodType +
            " preparation started."
        );

        float timer = 0f;

        while (timer < pickupTime)
        {
            timer += Time.deltaTime;

            if (loadingImage != null)
            {
                loadingImage.fillAmount =
                    Mathf.Clamp01(timer / pickupTime);
            }

            yield return null;
        }

        if (currentPlayer == null)
        {
            DisableLoadingImage();

            pickupCoroutine = null;
            yield break;
        }

        if (currentPlayer.IsCarrying)
        {
            DisableLoadingImage();

            pickupCoroutine = null;
            yield break;
        }

        currentPlayer.PickUpFood(
            foodPrefab,
            foodType
        );

        // The food is ready to hand to the employee.
        Debug.Log(
            foodType +
            " is ready."
        );

        DisableLoadingImage();

        EmployeeMovement employee =
            currentPlayer.GetComponentInParent<EmployeeMovement>();

        if (employee != null)
        {
            employee.FoodReady();
        }

        pickupCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        // If the player leaves, cancel the current food prep.
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (playerCarry != currentPlayer)
            return;

        if (pickupCoroutine != null)
        {
            Debug.Log(
                "Left " +
                foodType +
                " station before food was ready."
            );

            EmployeeMovement employee =
                playerCarry.GetComponentInParent<EmployeeMovement>();

            if (employee != null)
            {
                employee.FoodPreparationCancelled();
            }

            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;

            DisableLoadingImage();
        }

        currentPlayer = null;
    }

    private void StartLoadingImage()
    {
        if (loadingImage == null)
            return;

        loadingImage.fillAmount = 0f;
        loadingImage.gameObject.SetActive(true);

        // Show the loading bar for this station.
        Debug.Log(
            gameObject.name +
            ": Loading image ENABLED"
        );
    }

    private void DisableLoadingImage()
    {
        if (loadingImage == null)
            return;

        loadingImage.fillAmount = 0f;
        loadingImage.gameObject.SetActive(false);
    }
}