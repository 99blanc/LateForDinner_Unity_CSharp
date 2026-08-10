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
    private bool _isOpen = false;
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
    {
        _dropdownGroup?.Register(this);
    }

    private void OnDisable()
    {
        _dropdownGroup?.Unregister(this);
    }

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
        if (CaptionText != null)
        {
            if (Options.Count > 0 && _value >= 0 && _value < Options.Count)
                CaptionText.text = Options[_value].text;
            else
                CaptionText.text = string.Empty;
        }
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

        if (!Template.horizontal && !Template.vertical && Template.content != null)
        {
            float itemHeight = 0f;

            if (_itemPool.Count > 0 && _itemPool[0] != null)
            {
                if (_itemPool[0].TryGetComponent<RectTransform>(out var rectTransform))
                    itemHeight = rectTransform.rect.height;
            }

            float totalHeight = itemHeight * Options.Count;
            var templateRect = Template.transform as RectTransform;

            if (templateRect != null)
            {
                Vector2 size = templateRect.sizeDelta;
                size.y = totalHeight;
                templateRect.sizeDelta = size;
            }
        }

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
        foreach (var option in options)
            Options.Add(new OptionData { text = option });

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

        for (int i = 0; i < Options.Count; i++)
        {
            int index = i;
            GameObject itemObj;

            if (i < _itemPool.Count)
                itemObj = _itemPool[i];
            else
            {
                if (i == 0)
                    itemObj = itemTemplate.gameObject;
                else
                    itemObj = Instantiate(itemTemplate.gameObject, Template.content);

                _itemPool.Add(itemObj);
            }

            itemObj.SetActive(true);
            var textComponent = itemObj.GetComponentInChildren<TMP_Text>();

            if (textComponent != null)
                textComponent.text = Options[i].text;

            var toggle = itemObj.GetComponent<Toggle>();

            if (toggle != null)
            {
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
        }
    }

    private void HideAllItems()
    {
        foreach (var item in _itemPool)
        {
            if (item != null)
                item.SetActive(false);
        }
    }
}