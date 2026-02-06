using UnityEngine;

public class Define
{
    public const string ROOT = "@Root";
    public const string USER = "user";
    public const string CONFIG = ".config";
    public const string TEMP = ".tmp";

    public class Path
    {
        public const string SYSTEM = "Assets/Systems/";
        public const string SPRITE = "Assets/Sprites/";
        public const string ATLAS = "Assets/Atlas/";
        public const string PREFAB = "Assets/Prefabs/";
        public const string ANIMATOR = "Assets/Animators/";
        public const string TABLE = "Assets/Tables/";
    }

    public class Asset
    {
        public const string INPUT_SYSTEM = "InputSystem_Actions.inputactions";
        public const string TABLE_PLAYER = "Tables - Player.csv";
        public const string TABLE_LOCALIZATION_UI = "Tables - Localization_UI.csv";
        public const string TABLE_LOCALIZATION_STAT = "Tables - Localization_Stat.csv";
        public const string TABLE_LOCALIZATION_DIALOGUE = "Tables - Localization_Dialogue.csv";
        public const string PREFAB_PLAYER = "Agent/Player.prefab";
        public const string ANIMATOR_PLAYER = "Player/PlayerAnimator.controller";
    }

    public class Input
    {
        public const string MAP_USER = "User";
        public const string MAP_UI = "UI";
        public const string ACTION_MOVE = "Move";
        public const string ACTION_JUMP = "Jump";
        public const string ACTION_DASH = "Dash";
        public const string ACTION_ATTACK = "Attack";
    }

    public class Layer
    {
        public const string AGENT = "Agent";
        public const string PLAYER = "Player";
        public const string GROUND = "Ground";
        public const string LADDER = "Ladder";
        public const string BOX = "Box";
        public static readonly int GROUND_MASKS = LayerMask.GetMask(GROUND, BOX);
    }

    public class Physics
    {
        public const float HALF = 0.5f;
        public const float FULL = 1.0f;
        public const float DOUBLE = 2.0f;
        public const float LIMIT = 0.9f;
        public const float INTERVAL = 0.25f;
        public const float DEADZONE = 0.01f;
        public const float OFFSET = 0.25f;
        public const float FOOT = 0.8f;
        public const float SNAP = 0.05f;
        public const float BUFFER = 0.15f;
        public const float SLOPE = 45f;
        public const float TICK = 0.1f;
    }
}
