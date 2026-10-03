public static class FoodPricing
{
    public static int GetPrice(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Fries:
                return 5;

            case FoodType.IceCream:
                return 10;

            case FoodType.Burger:
                return 15;

            default:
                return 0;
        }
    }
}