using UnityEngine;

public class Define
{
    public class Alert
    {
        public const int Count = 5;
    }

    public class Log
    {
        public const int Storage = 1000;
    }

    public class Command
    {
        public const int History = 50;
    }

    public class Framerate
    {
        public const int Start = 60;
        public const int Step = 20;
        public const float PollingTime = 0.5f;
    }

    public class Save
    {
        public const int Amount = 8;
    }

    public class Cursor
    {
        public static readonly Vector2 Hotspot = new Vector2(26f, 36f);
    }

    public class Scaler
    {
        public static readonly Vector2 Resolution = new Vector2(3840f, 2160f);
        public const float PixelsPerUnit = 200f;
        public const float Margin = 0.975f;
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
        public const string Slot_Normal = "ui_common_image_slot_normal";
        public const string Slot_New = "ui_common_image_slot_new";
        public const string Slot_Highlight = "ui_common_image_slot_highlight";
        public const string Slot_Disable = "ui_common_image_slot_disable";
        public const string MealTime_Breakfast = "ui_common_image_mealtime_breakfast";
        public const string MealTime_Lunch = "ui_common_image_mealtime_lunch";
        public const string MealTime_Dinner = "ui_common_image_mealtime_dinner";
        public const string Checkmark_Yes = "ui_common_image_yes";
        public const string Checkmark_No = "ui_common_image_no";
    }

    public class Execute
    {
        public const string Console = "-console";
        public const string Debug = "-debug";
    }

    public class Atlas
    {
        public const string Common = "common_atlas";
    }
}
