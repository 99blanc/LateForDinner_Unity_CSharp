using System.Collections.Generic;
using UnityEngine;

public class DropdownGroup : MonoBehaviour
{
    private readonly List<Dropdown> _dropdowns = new List<Dropdown>();
    private Dropdown _currentOpenDropdown;

    public void Register(Dropdown dropdown)
    {
        if (!_dropdowns.Contains(dropdown))
            _dropdowns.Add(dropdown);
    }

    public void Unregister(Dropdown dropdown)
    {
        if (_dropdowns.Contains(dropdown))
            _dropdowns.Remove(dropdown);
    }

    public void NotifyDropdownOpened(Dropdown openedDropdown)
    {
        foreach (var dropdown in _dropdowns)
        {
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
