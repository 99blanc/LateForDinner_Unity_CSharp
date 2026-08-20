using System;
using TMPro;
using UnityEngine;

public class InputField : TMP_InputField
{
    public Action<string> OnSubmitAction;

    private void OnGUI()
    {
        if (isFocused && Event.current.isKey)
        {
            KeyCode key = Event.current.keyCode;

            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
                Event.current.Use();

            if (key == KeyCode.Return || key == KeyCode.KeypadEnter)
            {
                Event.current.Use();
                string currentText = text;
                OnSubmitAction?.Invoke(currentText);
                text = string.Empty;
                Select();
                ActivateInputField();
            }
        }
    }
}
