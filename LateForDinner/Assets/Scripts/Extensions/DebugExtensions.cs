using UnityEngine;

public static class DebugExtensions
{
    private static LineRenderer _debugLine;
    public static bool IsDebugDrawEnabled { get; set; } = false;

    public static bool ToggleDebugView()
    {
        IsDebugDrawEnabled = !IsDebugDrawEnabled;
        return IsDebugDrawEnabled;
    }

    public static void DrawDebugBoxLines(this Character character, Vector2 worldPosition, Vector2 size, Color color, bool isEnabled)
    {
        if (character == null)
            return;

        if (_debugLine == null || _debugLine.transform.parent != character.transform)
        {
            GameObject obj = new GameObject("Debug_Ground");
            obj.transform.SetParent(character.transform);
            _debugLine = obj.AddComponent<LineRenderer>();
            _debugLine.positionCount = 5;
            _debugLine.startWidth = 0.05f;
            _debugLine.endWidth = 0.05f;
            _debugLine.useWorldSpace = false;
            _debugLine.alignment = LineAlignment.TransformZ;
            _debugLine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            _debugLine.sortingOrder = 999;
        }

        _debugLine.SetActive(isEnabled);

        if (!isEnabled)
            return;

        _debugLine.transform.position = new Vector3(worldPosition.x, worldPosition.y, -0.1f);
        _debugLine.startColor = color;
        _debugLine.endColor = color;
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        _debugLine.SetPosition(0, new Vector3(-halfW, -halfH, 0f));
        _debugLine.SetPosition(1, new Vector3(halfW, -halfH, 0f));
        _debugLine.SetPosition(2, new Vector3(halfW, halfH, 0f));
        _debugLine.SetPosition(3, new Vector3(-halfW, halfH, 0f));
        _debugLine.SetPosition(4, new Vector3(-halfW, -halfH, 0f));
    }
}
