using UnityEngine;

public class CustomerEmoji : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Food Sprites")]
    [SerializeField] private Sprite burgerSprite;
    [SerializeField] private Sprite friesSprite;
    [SerializeField] private Sprite iceCreamSprite;

    [Header("Mood Sprites")]
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite angrySprite;

    public void ShowFoodRequest(FoodType foodType)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("CustomerEmoji: SpriteRenderer is not assigned!");
            return;
        }

        switch (foodType)
        {
            case FoodType.Burger:
                spriteRenderer.sprite = burgerSprite;
                break;

            case FoodType.Fries:
                spriteRenderer.sprite = friesSprite;
                break;

            case FoodType.IceCream:
                spriteRenderer.sprite = iceCreamSprite;
                break;
        }

        Debug.Log("Emoji showing order: " + foodType);
    }

    public void ShowResult(bool correctOrder)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("CustomerEmoji: SpriteRenderer is not assigned!");
            return;
        }

        if (correctOrder)
        {
            spriteRenderer.sprite = happySprite;
            Debug.Log("Emoji changed to HAPPY");
        }
        else
        {
            spriteRenderer.sprite = angrySprite;
            Debug.Log("Emoji changed to ANGRY");
        }
    }
}