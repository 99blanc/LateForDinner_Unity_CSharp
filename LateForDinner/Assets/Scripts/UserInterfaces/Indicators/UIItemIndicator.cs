using R3;
using UnityEngine;

public class UIItemIndicator : UIIndicator
{
    private enum Texts
    {
        ItemText
    }

    private Transform _targetTransform;
    private Vector3 _worldOffset;
    private RectTransform _parentRectTransform;

    public override void OnInit()
    {
        base.OnInit();
        BindText(typeof(Texts));
    }

    public override void OnGet()
    {
        base.OnGet();
        _targetTransform = default;
        _worldOffset = Vector3.zero;

        if (RectTransform.parent is RectTransform rect)
            _parentRectTransform = rect;

        Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
        .Where(_ => _targetTransform != null)
        .Subscribe(_ => UpdatePosition())
        .RegisterToPool(this);
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        _targetTransform = default;
    }

    public override Vector3? GetTargetWorldPosition()
    {
        if (_targetTransform == null)
            return null;

        return _targetTransform.position + _worldOffset;
    }

    public void SetTarget(Transform target, Vector3? customOffset = null)
    {
        _targetTransform = target;

        if (customOffset.HasValue)
            _worldOffset = customOffset.Value;
    }

    private void UpdatePosition()
    {
        var mainCamera = Managers.Camera?.Main;

        if (mainCamera == null || _parentRectTransform == null || !GetTargetWorldPosition().HasValue)
            return;

        Vector3 worldPosition = GetTargetWorldPosition().Value;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0f)
        {
            if (gameObject.activeSelf)
                this.SetActive(false);

            return;
        }

        if (!gameObject.activeSelf)
            this.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, screenPosition, null, out Vector2 localPoint);
        Vector2 finalLocalPoint = Managers.UI.GetStackedIndicatorPosition(this, localPoint, mainCamera);
        RectTransform.anchoredPosition = finalLocalPoint;
    }

    public void SetItemName(string itemName)
    {
        GetText(Texts.ItemText).text = itemName;
        float requiredWidth = GetText(Texts.ItemText).preferredWidth;
        float requiredHeight = GetText(Texts.ItemText).preferredHeight;
        RectTransform.sizeDelta = new Vector2(requiredWidth, requiredHeight);
    }
}
