using Unity.Cinemachine;
using UnityEngine;

public class CameraManager
{
    private GameObject _root;

    public GameObject Root
    {
        get
        {
            if (_root == null)
                InitRoot();

            return _root;
        }
    }
    private CinemachineCamera _vcam;
    private Camera _mainCamera;
    private CinemachineFollow _follow;
    private CameraWorkMode _currentMode;

    private void InitRoot()
    {
        GameObject root = new GameObject { name = Literal.Roots.Camera };
        root.transform.SetParent(Managers.Instance.transform, false);
        _mainCamera = root.AddComponent<Camera>();
        _mainCamera.tag = Literal.Tags.Camera;
        _mainCamera.clearFlags = CameraClearFlags.SolidColor;
        _mainCamera.backgroundColor = Color.black;
        _mainCamera.orthographic = true;
        root.AddComponent<CinemachineBrain>();
        GameObject vcam = new GameObject { name = Literal.Roots.Virtual };
        vcam.transform.SetParent(Managers.Instance.transform, false);
        _vcam = vcam.AddComponent<CinemachineCamera>();
        _follow = vcam.AddComponent<CinemachineFollow>();
        _vcam.Lens.OrthographicSize = 5f;
    }

    public void Setup()
    {
        var _ = Root;
        SetCameraMode(CameraWorkMode.CupheadLightFocus);
        Log.System(LocalizationKey.Log_Camera_LoadSuccess);
    }

    public void SetTarget(PlayableCharacter character)
    {
        if (_vcam == null || character == null)
            return;

        _vcam.Follow = character.CameraTransform;
    }

    public void SetCameraMode(CameraWorkMode mode)
    {
        if (_follow == null)
            return;

        _currentMode = mode;

        switch (mode)
        {
            case CameraWorkMode.MetroidWindowSpeedUp:
                ApplyMetroidWindowSpeedUp();
                break;
            case CameraWorkMode.JazzTargetFocus:
                ApplyJazzTargetFocus();
                break;

            case CameraWorkMode.CaveStorySmoothDualFocus:
                ApplyCaveStorySmoothDualFocus();
                break;

            case CameraWorkMode.CupheadLightFocus:
                ApplyCupheadLightFocus();
                break;
        }
    }

    private void ApplyMetroidWindowSpeedUp()
    {
        // TODO: 메트로이드 스타일 구현
        _follow.TrackerSettings.PositionDamping = new Vector3(2.5f, 2.5f, 0f);
    }

    private void ApplyJazzTargetFocus()
    {
        // TODO: 재즈 잭 래빗 2 스타일 구현
        _follow.TrackerSettings.PositionDamping = new Vector3(1.2f, 1.2f, 0f);
    }

    private void ApplyCaveStorySmoothDualFocus()
    {
        // TODO: 동굴 이야기 스타일 구현
        _follow.TrackerSettings.PositionDamping = new Vector3(4.0f, 3.0f, 0f);
    }

    private void ApplyCupheadLightFocus()
    {
        // TODO: 컵헤드 스타일 구현
        _follow.TrackerSettings.PositionDamping = new Vector3(0.4f, 0.4f, 0f);
    }
}