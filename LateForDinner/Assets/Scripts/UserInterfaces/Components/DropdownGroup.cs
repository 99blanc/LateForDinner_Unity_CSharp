using System.Collections.Generic;
using UnityEngine;

public class DropdownGroup : MonoBehaviour
{
    private readonly List<Dropdown> _dropdowns = new List<Dropdown>();
    private Dropdown _currentOpenDropdown;

    public void Register(Dropdown dropdown)
    {
        if (dropdown == null || _dropdowns.Contains(dropdown))
            return;

        _dropdowns.Add(dropdown);
    }

    public void Unregister(Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        _dropdowns.Remove(dropdown);

        if (_currentOpenDropdown == dropdown)
            _currentOpenDropdown = null;
    }

    public void NotifyDropdownOpened(Dropdown openedDropdown)
    {
        if (openedDropdown == null)
            return;

        for (int index = 0; index < _dropdowns.Count; index++)
        {
            var dropdown = _dropdowns[index];

            if (dropdown != openedDropdown && dropdown.IsOpen)
                dropdown.Close();
        }

        _currentOpenDropdown = openedDropdown;
    }

    public void NotifyDropdownClosed(Dropdown closedDropdown)
    {
        if (_currentOpenDropdown == closedDropdown)
            _currentOpenDropdown = null;
    }
}
