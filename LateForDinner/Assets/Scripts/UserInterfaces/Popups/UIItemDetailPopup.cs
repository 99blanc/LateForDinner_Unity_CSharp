using LateForDinner.Data;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIItemDetailPopup : UIPopup
{
    private enum Images
    {
        SlotItemImage
    }

    private enum Texts
    {
        ItemNameText,
        ItemCategoryText,
        DescriptionText,
        FlavorText
    }

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        this.GetComponentAssert<CanvasGroup>().blocksRaycasts = false;
    }

    public override void OnGet()
    {
        base.OnGet();
        Observable.EveryUpdate()
        .Subscribe(_ => 
        {
            if (Mouse.current != null)
                UpdatePopupPosition(Mouse.current.position.ReadValue());
        }).RegisterToPool(this);
    }

    public void Setup(int itemID, Vector2 mousePosition)
    {
        if (!Managers.Data.Items.TryGetValue(itemID, out ItemData itemData))
            return;

        GetImage(Images.SlotItemImage).SetActive(true);
        GetImage(Images.SlotItemImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Item, itemData.AddressableKey);
        GetText(Texts.ItemNameText).text = Managers.Localization.Get(itemData.NameKey);
        GetText(Texts.DescriptionText).text = Managers.Localization.Get(itemData.DescriptionKey);
        GetText(Texts.FlavorText).text = Managers.Localization.Get(itemData.FlavorKey);

        string categoryText = string.Empty;

        if (Managers.Data.ItemCategories.TryGetValue(itemData.ItemCategory, out ItemCategoryData itemCategoryData))
            categoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);

        if (Managers.Data.ArmorItems.TryGetValue(itemID, out var armorItem) && Managers.Data.ArmorCategories.TryGetValue(armorItem.ArmorCategory, out ArmorCategoryData armorCategoryData))
        {
            string itemCategoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);
            string armorCategoryText = Managers.Localization.Get(armorCategoryData.LocalizationKey);
            categoryText = Managers.Localization.Get(LocalizationKey.Item_Equipment_Format, itemCategoryText, armorCategoryText);
        }

        if (Managers.Data.WeaponItems.TryGetValue(itemID, out var weaponItem) && Managers.Data.WeaponCategories.TryGetValue(weaponItem.WeaponCategory, out WeaponCategoryData weaponCategoryData))
        {
            string itemCategoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);
            string weaponCategoryText = Managers.Localization.Get(weaponCategoryData.LocalizationKey);
            categoryText = Managers.Localization.Get(LocalizationKey.Item_Equipment_Format, itemCategoryText, weaponCategoryText);
        }

        GetText(Texts.ItemCategoryText).text = categoryText;
        RectTransform.pivot = Define.UI.ItemDetailPopup;
        UpdatePopupPosition(mousePosition);
    }

    private void UpdatePopupPosition(Vector2 mousePosition)
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.GetComponentAssert<RectTransform>();
        RectTransform popupRect = RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePosition, cam, out Vector2 localPoint);
        Vector2 popupSize = popupRect.rect.size;
        Vector2 pivot = popupRect.pivot;
        Rect canvasArea = canvasRect.rect;
        float minX = localPoint.x - (popupSize.x * pivot.x);
        float maxX = localPoint.x + (popupSize.x * (1f - pivot.x));
        float minY = localPoint.y - (popupSize.y * pivot.y);
        float maxY = localPoint.y + (popupSize.y * (1f - pivot.y));
        float offsetX = 0f;
        float offsetY = 0f;

        if (minX < canvasArea.xMin)
            offsetX = canvasArea.xMin - minX;

        if (maxX > canvasArea.xMax)
            offsetX = canvasArea.xMax - maxX;

        if (minY < canvasArea.yMin)
            offsetY = canvasArea.yMin - minY;

        if (maxY > canvasArea.yMax)
            offsetY = canvasArea.yMax - maxY;

        popupRect.anchoredPosition = localPoint + new Vector2(offsetX, offsetY);
    }
}
