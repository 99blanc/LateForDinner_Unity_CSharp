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
        public const string PREFAB_PLAYER = "Character/Player.prefab";
    }

    public class Input
    {
        public const float INPUT_THRESHOLD = 0.01f;
        public const float INPUT_BUFFER_TIME = 0.9f;
        public const float INPUT_DOUBLE_TAP_TIME = 0.3f;
        public const string MAP_USER = "User";
        public const string MAP_UI = "UI";
        public const string ACTION_MOVE = "Move";
        public const string ACTION_JUMP = "Jump";
        public const string ACTION_DASH = "Dash";
        public const string ACTION_ATTACK = "Attack";
    }

    public class Layer
    {
        public const string GROUND = "Ground";
    }
}
