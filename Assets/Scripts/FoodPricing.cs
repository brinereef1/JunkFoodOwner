public static class FoodPricing
{
    public static int GetPrice(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Burger:
                return 5;

            case FoodType.IceCream:
                return 3;

            case FoodType.Fries:
                return 2;

            default:
                return 0;
        }
    }
}