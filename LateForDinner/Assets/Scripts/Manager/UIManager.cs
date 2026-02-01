using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private readonly Vector3 DEFAULT_SCALE = Vector3.one;
    private Stack<UIPopup> popups = new();
    private GameObject container = new(Define.ROOT);
    private int curCanvasOrder = -20;

    public void Init() => popups.Clear();

    public void SetCanvas(GameObject gameObject, bool sort = true)
    {
        Canvas canvas = gameObject.GetComponentAssert<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = curCanvasOrder;
            curCanvasOrder += 1;
            return;
        }

        canvas.sortingOrder = 0;
    }

    public async UniTask<T> OpenPopup<T>(Transform parent = null) where T : UIPopup
    {
        T popup = await SetupUI<T>(parent);
        popups.Push(popup);
        return popup;
    }

    public async UniTask<T> OpenSubItem<T>(Transform parent = null) where T : UISubItem => await SetupUI<T>(parent);

    private async UniTask<T> SetupUI<T>(Transform parent = null) where T : UserInterface
    {
        GameObject prefab = await Managers.Resource.LoadPrefab(typeof(T).Name);
        GameObject gameObject = Managers.Resource.Instantiate(prefab);
        T UI = gameObject.GetComponentAssert<T>();
        UI.Init();
        Transform transform = gameObject.transform;
        transform.localScale = DEFAULT_SCALE;
        transform.localPosition = prefab.transform.position;
        Transform targetParent = parent is null ? container.transform : parent;
        transform.SetParent(targetParent);

        if (UI is UIPopup)
            SetCanvas(gameObject);

        return UI;
    }

    public void ClosePopup(UIPopup popup)
    {
        if (popups.Count == 0 || popups.Peek() != popup)
            return;

        ClosePopup();
    }

    public void ClosePopup()
    {
        if (popups.Count == 0)
            return;

        UIPopup popup = popups.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        curCanvasOrder -= 1;
    }

    public void CloseAllPopup()
    {
        while (popups.Count > 0)
            ClosePopup();
    }
}
