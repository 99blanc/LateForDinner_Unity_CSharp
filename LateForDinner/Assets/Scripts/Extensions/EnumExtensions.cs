public static class EnumExtensions
{
    public static string ToSpriteAsMealTime(this MealTime mealTime)
    {
        return mealTime switch
        {
            MealTime.Breakfast => Define.Sprite.MealTime_Breakfast,
            MealTime.Lunch => Define.Sprite.MealTime_Lunch,
            MealTime.Dinner => Define.Sprite.MealTime_Dinner,
            _ => Define.Sprite.MealTime_Breakfast
        };
    }
}
