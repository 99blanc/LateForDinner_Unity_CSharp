public class Define
{
    public class Amount
    {
        public const int MaxSaveSlot = 4;
        public const int MaxQuickSlot = 4;
        public const int MaxDashCount = 28;
        public const int MaxHealthCount = 56;
    }

    public class Animation
    {
        public static readonly int None = UnityEngine.Animator.StringToHash("None");
        // DESC ::: Character Animator 해시
        public static readonly int Idle = UnityEngine.Animator.StringToHash("Idle");
        public static readonly int Move = UnityEngine.Animator.StringToHash("Move");
        public static readonly int Fall = UnityEngine.Animator.StringToHash("Fall");
        public static readonly int Crouch = UnityEngine.Animator.StringToHash("Crouch");
        public static readonly int Jump = UnityEngine.Animator.StringToHash("Jump");
        public static readonly int DoubleJump = UnityEngine.Animator.StringToHash("DoubleJump");
        public static readonly int Roll = UnityEngine.Animator.StringToHash("Roll");
        public static readonly int Dash = UnityEngine.Animator.StringToHash("Dash");
        public static readonly int DownDash = UnityEngine.Animator.StringToHash("DownDash");
        public static readonly int Climb = UnityEngine.Animator.StringToHash("Climb");
        public static readonly int PickupTrayIdle = UnityEngine.Animator.StringToHash("PickupTrayIdle");
        public static readonly int PickupTrayMove = UnityEngine.Animator.StringToHash("PickupTrayMove");
        public static readonly int PickupTrayFall = UnityEngine.Animator.StringToHash("PickupTrayFall");
        public static readonly int PickupTrayJump = UnityEngine.Animator.StringToHash("PickupTrayJump");
        public static readonly int ThrowTray = UnityEngine.Animator.StringToHash("ThrowTray");
        // DESC ::: HeadUpDisplay Animator 해시
        public static readonly int HeadUpHealthFull = UnityEngine.Animator.StringToHash("UIHeadUpDisplay_HealthFull");
        public static readonly int HeadUpHealthHalf = UnityEngine.Animator.StringToHash("UIHeadUpDisplay_HealthHalf");
        public static readonly int HeadUpHealthHelp = UnityEngine.Animator.StringToHash("UIHeadUpDisplay_HealthHelp");
        public static readonly int HeadUpDashCharge = UnityEngine.Animator.StringToHash("UIHeadUpDisplay_DashCharge");
        public static readonly int HeadUpDashUse = UnityEngine.Animator.StringToHash("UIHeadUpDisplay_DashUse");
        public const float NormalizedTime = 0.95f;
    }

    public class Animator
    {
        public const string UIAnimator = "UIAnimator";
    }

    public class Atlas
    {
        public const string Common = "common_atlas";
        public const string Title = "splash_title_atlas";
        public const string Load = "load_atlas";
        public const string PlayableCharacter = "playablecharacter_atlas";
        public const string HeadUp = "headup_atlas";
    }

    public class Command
    {
        public const int History = 100;
    }

    public class Cursor
    {
        public static readonly UnityEngine.Vector2 Hotspot = new UnityEngine.Vector2(26f, 36f);
        public const float Duration = 5f;
    }

    public class Day
    {
        public const int Start = 1;
    }

    public class Execute
    {
        public const string Console = "-console";
        public const string Debug = "-debug";
    }

    public class Framerate
    {
        public const float PollingTime = 0.5f;
        public const int Start = 60;
        public const int Step = 20;
    }

    public class Log
    {
        public const int Storage = 1000;
    }

    public class Scaler
    {
        public const float Buffer = 0.4f;
        public const float Threshold = 0.2f;
        public const float Duration = 0.15f;
        public const float Margin = 0.975f;
        public const float PixelsPerUnit = 200f;
        public static readonly UnityEngine.Vector2 Resolution = new UnityEngine.Vector2(3840f, 2160f);
    }

    public class Sprite
    {
        public const string Button_Arrow_Disable = "ui_common_button_arrow_disable";
        public const string Button_Arrow_Highlight = "ui_common_button_arrow_highlight";
        public const string Button_Arrow_Normal = "ui_common_button_arrow_normal";
        public const string Button_Arrow_Press = "ui_common_button_arrow_press";
        public const string Button_Disable = "ui_common_button_disable";
        public const string Button_Highlight = "ui_common_button_highlight";
        public const string Button_New = "ui_common_button_new";
        public const string Button_Normal = "ui_common_button_normal";
        public const string Button_Press = "ui_common_button_press";
        public const string Checkmark_No = "ui_common_image_no";
        public const string Checkmark_Yes = "ui_common_image_yes";
        public const string Cursor_Normal = "ui_common_cursor_normal";
        public const string Cursor_Press = "ui_common_cursor_press";
        public const string MealTime_Breakfast = "ui_common_image_mealtime_breakfast";
        public const string MealTime_Dinner = "ui_common_image_mealtime_dinner";
        public const string MealTime_Lunch = "ui_common_image_mealtime_lunch";
        public const string Slot_Disable = "ui_common_image_slot_disable";
        public const string Slot_Highlight = "ui_common_image_slot_highlight";
        public const string Slot_New = "ui_common_image_slot_new";
        public const string Slot_Normal = "ui_common_image_slot_normal";

        public const string HUD_PlayerDashCount = "ui_headup_player_dash_count";
        public const string HUD_PlayerHealth_Empty = "ui_headup_player_health_empty";
        public const string HUD_PlayerHealth_Half = "ui_headup_player_health_half";
        public const string HUD_PlayerHealth_Full = "ui_headup_player_health_full";
        public const string HUD_PlayerTemporaryHealth_Half = "ui_headup_player_temporary_health_half";
        public const string HUD_PlayerTemporartHealth_Full = "ui_headup_player_temporary_health_full";
    }

    public class Toast
    {
        public const int Count = 5;
        public const float Delay = 0.2f;
    }
}
