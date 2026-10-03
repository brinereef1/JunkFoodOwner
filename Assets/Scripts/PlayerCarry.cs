using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    private GameObject carriedFood;
    private FoodType carriedFoodType;

    public bool IsCarrying => carriedFood != null;

    public FoodType CarriedFoodType => carriedFoodType;

    public GameObject TakeFood()
    {
        if (!IsCarrying)
            return null;

        GameObject food = carriedFood;

        carriedFood = null;

        return food;
    }

    public void PickUpFood(
        GameObject foodPrefab,
        FoodType foodType)
    {
        if (IsCarrying)
            return;

        carriedFoodType = foodType;

        carriedFood = PoolManager.Instance.Get(
            foodPrefab,
            transform.position,
            transform.rotation
        );

        // Keep the exact world position and rotation
        // where the food spawned.
        carriedFood.transform.SetParent(
            transform,
            true
        );

        Rigidbody rb =
            carriedFood.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("Picked up " + foodType);
    }
}