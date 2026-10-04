using System.Collections;
using UnityEngine;

public class KitchenStation : MonoBehaviour
{
    [SerializeField] private FoodType foodType;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float pickupTime = 2f;

    public FoodType FoodType => foodType;

    private PlayerCarry currentPlayer;
    private Coroutine pickupCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        // Someone is already using this station
        if (currentPlayer != null)
            return;

        // Player/employee already carrying something
        if (playerCarry.IsCarrying)
            return;

        // ---------------------------------------------
        // Everything passed.
        // NOW this station accepts the interaction.
        // ---------------------------------------------

        currentPlayer = playerCarry;

        // Check if this is an employee
        EmployeeMovement employee =
            playerCarry.GetComponentInParent<EmployeeMovement>();

        if (employee != null)
        {
            // Only NOW stop the employee.
            employee.StartFoodPreparation(this);
        }

        pickupCoroutine =
            StartCoroutine(PickupFood());
    }

    private IEnumerator PickupFood()
    {
        Debug.Log(
            foodType +
            " preparation started."
        );

        yield return new WaitForSeconds(pickupTime);

        // Nobody is using the station anymore
        if (currentPlayer == null)
        {
            pickupCoroutine = null;
            yield break;
        }

        // Someone already picked something up
        if (currentPlayer.IsCarrying)
        {
            pickupCoroutine = null;
            yield break;
        }

        // Give food
        currentPlayer.PickUpFood(
            foodPrefab,
            foodType
        );

        Debug.Log(
            foodType +
            " is ready."
        );

        // If employee, tell employee to continue.
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
        PlayerCarry playerCarry =
            other.GetComponent<PlayerCarry>();

        if (playerCarry == null)
            return;

        if (playerCarry != currentPlayer)
            return;

        // IMPORTANT:
        // If preparation is still running,
        // the person actually left before food was ready.
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
        }

        currentPlayer = null;
    }
}