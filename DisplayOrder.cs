using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOrder : MonoBehaviour
{
    public Camera mainCamera;
    public Dropdown displayDropdown;

    private void Start()
    {
        // 启用所有显示器
        for (int i = 0; i < 4; i++)
        {
            Display.displays[i].Activate();
        }
        // 添加选项到Dropdown
        List<Dropdown.OptionData> displayOptions = new List<Dropdown.OptionData>();
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData("Display " + i, null);
            displayOptions.Add(option);
        }
        displayDropdown.AddOptions(displayOptions);
        // 设置默认显示器
        if (Display.displays.Length > 0)
        {
            mainCamera.targetDisplay = 0; // 默认显示器为第一个
        }
}

    public void OnDisplayDropdownChanged()
    {
        int selectedDisplay = displayDropdown.value;
        mainCamera.targetDisplay = selectedDisplay;
    }
}