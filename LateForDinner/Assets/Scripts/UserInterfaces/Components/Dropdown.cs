using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dropdown : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Mappings")]
    public ScrollRect Template;
    public TMP_Text CaptionText;
    public Image CaptionImage;
    public TMP_Text ItemText;
    public Image ItemImage;
    public Button Button;
    public Image ButtonImage;

    [Header("Group")]
    [SerializeField] private DropdownGroup _dropdownGroup;

    [Header("Settings")]
    [SerializeField] private int _value;
    public List<OptionData> Options = new List<OptionData>();

    [Serializable]
    public class OptionData
    {
        public string text;
        public Sprite image;
    }

    [Serializable]
    public class DropdownEvent : UnityEvent<int> { }
    public DropdownEvent onValueChanged = new DropdownEvent();

    private bool _isOpen;
    public bool IsOpen => _isOpen;
    private readonly List<GameObject> _itemPool = new List<GameObject>();

    public int value
    {
        get => _value;
        set
        {
            _value = value;
            Refresh();
            onValueChanged?.Invoke(_value);
        }
    }

    private void Awake()
    {
        if (_dropdownGroup == null)
            _dropdownGroup = GetComponentInParent<DropdownGroup>();
    }

    private void OnEnable()
        => _dropdownGroup?.Register(this);

    private void OnDisable()
        => _dropdownGroup?.Unregister(this);

    private void Start()
    {
        if (Template != null)
            Template.gameObject.SetActive(false);

        if (Button != null)
            Button.onClick.AddListener(Toggle);

        Refresh();
    }

    private void Refresh()
    {
        if (CaptionText == null)
            return;

        if (Options.Count > 0 && _value >= 0 && _value < Options.Count)
        {
            CaptionText.text = Options[_value].text;
            return;
        }

        CaptionText.text = string.Empty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Toggle();
    }

    public void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (!isActiveAndEnabled || Template == null)
            return;

        _dropdownGroup?.NotifyDropdownOpened(this);
        BuildItemContainer();
        Template.gameObject.SetActive(true);
        AdjustTemplateHeight();
        _isOpen = true;
        UpdateArrowRotation();
    }

    public void Close()
    {
        if (!_isOpen)
            return;

        if (Template != null)
            Template.gameObject.SetActive(false);

        _isOpen = false;
        _dropdownGroup?.NotifyDropdownClosed(this);
        UpdateArrowRotation();
    }

    public void SelectOption(int index)
    {
        value = index;
        Close();
    }

    private void UpdateArrowRotation()
    {
        if (ButtonImage != null)
            ButtonImage.rectTransform.localRotation = Quaternion.Euler(0, 0, _isOpen ? 0f : 180f);
    }

    public void ClearOptions()
    {
        Options.Clear();
        HideAllItems();
    }

    public void AddOptions(List<string> options)
    {
        if (options == null)
            return;

        for (int index = 0; index < options.Count; index++)
            Options.Add(new OptionData { text = options[index] });

        Refresh();
    }

    private void BuildItemContainer()
    {
        if (Template == null || Template.content == null)
            return;

        HideAllItems();

        if (ItemText == null || ItemText.transform.parent == null)
            return;

        Transform itemTemplate = ItemText.transform.parent;

        for (int index = 0; index < Options.Count; index++)
        {
            int sub = index;
            GameObject itemObj = GetOrCreateItem(index, itemTemplate);
            itemObj.SetActive(true);
            UpdateItemContent(itemObj, sub);
        }
    }

    private GameObject GetOrCreateItem(int index, Transform itemTemplate)
    {
        if (index < _itemPool.Count)
            return _itemPool[index];

        GameObject itemObj = index == 0 ? itemTemplate.gameObject : Instantiate(itemTemplate.gameObject, Template.content);
        _itemPool.Add(itemObj);
        return itemObj;
    }

    private void UpdateItemContent(GameObject itemObj, int index)
    {
        var textComponent = itemObj.GetComponentInChildren<TMP_Text>();

        if (textComponent != null)
            textComponent.text = Options[index].text;

        var toggle = itemObj.GetComponentAssert<Toggle>();

        if (toggle == null)
            return;

        toggle.isOn = (index == _value);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                SelectOption(index);
            else if (index == _value)
                toggle.isOn = true;
        });
    }

    private void AdjustTemplateHeight()
    {
        if (Template.horizontal || Template.vertical || Template.content == null)
            return;

        float itemHeight = 0f;

        if (_itemPool.Count > 0 && _itemPool[0] != null)
        {
            if (_itemPool[0].TryGetComponent<RectTransform>(out var rectTransform))
                itemHeight = rectTransform.rect.height;
        }

        float totalHeight = itemHeight * Options.Count;

        if (Template.transform is RectTransform templateRect)
        {
            Vector2 size = templateRect.sizeDelta;
            size.y = totalHeight;
            templateRect.sizeDelta = size;
        }
    }

    private void HideAllItems()
    {
        for (int index = 0; index < _itemPool.Count; index++)
        {
            if (_itemPool[index] != null)
                _itemPool[index].SetActive(false);
        }
    }
}
