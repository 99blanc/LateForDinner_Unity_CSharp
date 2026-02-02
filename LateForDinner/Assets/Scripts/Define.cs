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
        public const string PREFAB = "Assets/Prefabs/";
        public const string TABLE = "Assets/Tables/";
    }

    public class Asset
    {
        public const string FILE_INPUT_SYSTEM = "InputSystem_Actions.inputactions";
        public const string FILE_PLAYER = "Player.csv";
        public const string PREFAB_PLAYER = "Agent/Player.prefab";
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
        public const float INTERVAL = 0.2f;
        public const float DEADZONE = 0.01f;
        public const float OFFSET = 0.1f;
        public const float SNAP = 0.05f;
        public const float BUFFER = 5f;
    }
}
