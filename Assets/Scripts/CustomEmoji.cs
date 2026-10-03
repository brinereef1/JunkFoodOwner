using UnityEngine;

public class CustomerEmoji : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Food Sprites")]
    [SerializeField] private Sprite burgerSprite;
    [SerializeField] private Sprite friesSprite;
    [SerializeField] private Sprite iceCreamSprite;

    [Header("Result Sprites")]
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite angrySprite;

    public void ShowFoodRequest(FoodType foodType)
    {
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
    }

    public void ShowResult(bool correctOrder)
    {
        if (correctOrder)
        {
            spriteRenderer.sprite = happySprite;
        }
        else
        {
            spriteRenderer.sprite = angrySprite;
        }
    }
}