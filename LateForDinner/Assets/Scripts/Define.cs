using UnityEngine;

public class Define
{
    public class Save
    {
        public const int Amount = 4;
        public static readonly int[] Orders = new int[] { 0, 1, 2, 3, };
    }

    public class Cursor
    {
        public static readonly Vector2 Hotspot = new Vector2(25f, 35f);
    }

    public class Sprite
    {
        public const string Cursor_Normal = "ui_common_cursor_normal";
        public const string Cursor_Press = "ui_common_cursor_press";
        public const string Button_Normal = "ui_common_button_normal";
        public const string Button_New = "ui_common_button_new";
        public const string Button_Highlight = "ui_common_button_highlight";
        public const string Button_Press = "ui_common_button_press";
        public const string Button_Disable = "ui_common_button_disable";
        public const string Button_Arrow_Normal = "ui_common_button_arrow_normal";
        public const string Button_Arrow_Highlight = "ui_common_button_arrow_highlight";
        public const string Button_Arrow_Press = "ui_common_button_arrow_press";
        public const string Button_Arrow_Disable = "ui_common_button_arrow_disable";
        public const string MealTime_Breakfast = "ui_common_image_mealtime_breakfast";
        public const string MealTime_Lunch = "ui_common_image_mealtime_lunch";
        public const string MealTime_Dinner = "ui_common_image_mealtime_dinner";
    }

    public class Atlas
    {
        public const string UI_Common = "UI_Common";
    }
}
