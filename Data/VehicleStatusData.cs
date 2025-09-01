using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
#endif

/// <summary>
/// Data for the Vehicle
/// </summary>
[HelpURL("https://confluence.iavgroup.local/display/CNTTU/VehicleStatusData")]
[CreateAssetMenu(fileName = "VehicleData.asset", menuName = "IAV/VehicleData", order = 1)]
[HideMonoScript]
[Serializable]
public class VehicleStatusData : ScriptableObject
{
    [TitleGroup("Dials")]
    public float speed;
    [TitleGroup("Dials")]
    [Min(0)]
    public float rpm;

    [TitleGroup("Battery")]
    public bool plugged = false;
    [TitleGroup("Battery")]
    [MinValue(0)]
    public float batteryCapacity;
    [TitleGroup("Battery")]
    [PropertyRange(0, nameof(batteryCapacity))]
    public float batteryCurrent;
    [TitleGroup("Battery")]
    [PropertyRange(0, 100)]
    [ShowInInspector]
    public int batteryPercent
    {
        get
        {
            return Mathf.RoundToInt(batteryCurrent / batteryCapacity * 100);
        }
        set
        {
            batteryCurrent = (float)value / 100 * batteryCapacity;
        }
    }

    ///<summary>Current transmission</summary>
    [TitleGroup("Gears")]
    [EnumToggleButtons]
    public Transmission transmission;

    ///<summary>Number of D Gears</summary>
    [TitleGroup("Gears")]
#if UNITY_EDITOR
    [Min(1), InlineButton(nameof(ResetGearNum), "↺"), InlineButton(nameof(RaiseGearNum), ">"), InlineButton(nameof(LowerGearNum), "<")]
#endif
    public int driveGear = 6;
#if UNITY_EDITOR
    private void RaiseGearNum() { driveGear++; }
    private void LowerGearNum() { driveGear = driveGear > 1 ? driveGear - 1 : 1; }
    private void ResetGearNum() { driveGear = 6; }
#endif
    //Enum for PRND
    public enum Transmission
    {
        P,
        R,
        N,
        D
    }

    [TitleGroup("ODO")]
    public int mileage;
    [TitleGroup("ODO")]
    [PropertyRange(0, nameof(mileage))]
    public int tripStart;

    ///<summary>System.DateTime.Now</summary>
    [TitleGroup("Misc")]
    public bool newFPK = true;
    [HideInInspector]
    public bool _newFPK { get => newFPK; set => newFPK = value; }
    [TitleGroup("Misc")]
    [ShowInInspector, DisplayAsString]
    public DateTime dateTime = DateTime.Now;
#if UNITY_EDITOR
    [TitleGroup("Misc"), InlineButton("SwitchTempCF", "$" + nameof(tempCF))]
#endif
    public int temperature = 25;

    ///<summary>°C or °F</summary>
    [HideInInspector]
    public bool useC = true;
    [TitleGroup("Misc")]
    [Range(0, 100)]
    public int boostPercent;

#if UNITY_EDITOR
    private string tempCF { get { return useC ? "°C" : "°F"; } }

    private void SwitchTempCF() { useC = !useC; }
#endif

    //Used to load groups of indicators through right clicks
    [TitleGroup("Indicators"), PropertyOrder(0)]
    [ShowInInspector, CustomContextMenu("Add To List", nameof(AddAll)), CustomContextMenu("Replace List", nameof(ReplaceAll))]
    private GameObject indicatorsParent;
    private List<string> indicatorsNameList;

    //Add children indicators to the list
    private void AddAll()
    {
        if (indicatorsParent == null) return;

        indicatorsNameList = indicatorsParent
            .GetComponentsInChildren<RectTransform>(true)
            .Where(x => x.parent == indicatorsParent.transform)
            .Select(x => x.name).ToList();
        indicatorsNameList.Sort();

        foreach (string name in indicatorsNameList)
        {
            Indicator toAdd = new();
            toAdd.name = name;
            toAdd.nameList = indicatorsNameList;
            toggles.Add(toAdd);
        }
    }
    //Overrides the list with children indicators
    private void ReplaceAll()
    {
        if (indicatorsParent == null) return;

        indicatorsNameList = indicatorsParent
            .GetComponentsInChildren<RectTransform>(true)
            .Where(x => x.parent == indicatorsParent.transform)
            .Select(x => x.name).ToList();
        indicatorsNameList.Sort();

        toggles.Clear();

        foreach (string name in indicatorsNameList)
        {
            Indicator toAdd = new();
            toAdd.name = name;
            toAdd.nameList = indicatorsNameList;
            toggles.Add(toAdd);
        }
    }

    ///<summary>
    ///Indicators On/Off data
    ///</summary>
    [TitleGroup("Indicators"), PropertyOrder(2)]
#if UNITY_EDITOR
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 20, DrawScrollView = false),
        OnCollectionChanged(After = nameof(InitToggles)), //Custom actions
        CustomContextMenu("SetAll/On", nameof(SetAllOn)),
        CustomContextMenu("SetAll/Off", nameof(SetAllOff)),
        CustomContextMenu("SetAll/Reverse", nameof(SetAllReverse)),
        CustomContextMenu("RefreshGUI", nameof(RefreshGUI))]
#endif
    public List<Indicator> toggles = new();

#if UNITY_EDITOR
    //Name list will be the children of indicatorsParent
    private void InitToggles(CollectionChangeInfo info, List<Indicator> values)
    {
        if (info.ChangeType.Equals(CollectionChangeType.Add))
        {
            if (indicatorsNameList == null)
            {
                indicatorsNameList = indicatorsParent.GetComponentsInChildren<RectTransform>().Select(x => x.name).ToList();
                indicatorsNameList.Sort();
            }
            toggles.Last().nameList = indicatorsNameList;
        }
    }
    private void SetAllOn()
    {
        toggles.ForEach(x => { x.isOn = true; x.UpdateGUI(); });
    }
    private void SetAllOff()
    {
        toggles.ForEach(x => { x.isOn = false; x.UpdateGUI(); });
    }
    private void SetAllReverse()
    {
        toggles.ForEach(x => { x.isOn = !x.isOn; x.UpdateGUI(); });
    }
#endif

    //Reload button GUIs
    private void RefreshGUI()
    {
        toggles.ForEach(x => x.UpdateGUI());
    }

    /// <summary>
    /// Class for indicator's name and status
    /// </summary>
    [Serializable]
    public class Indicator
    {
        /// <summary>Used for name dropdown selection</summary>
        [HideInInspector]
        public List<string> nameList;
        [ValueDropdown("nameList", AppendNextDrawer = true, SortDropdownItems = true, DropdownWidth = 200)]
        public string name;
        [HorizontalGroup("Status", PaddingRight = 0)]
        [HideLabel]
        [HideInInspector]
        [OnValueChanged(nameof(UpdateGUI))]
        public bool isOn;
        [HorizontalGroup("Status")]
        [Button("$" + nameof(btnText)), GUIColor(nameof(col))]
        private void ColoredButton()
        {
            isOn = !isOn;
            UpdateGUI();
        }

        private Color col;
        private string btnText;

        [OnInspectorInit]
        public void UpdateGUI()
        {
            col = isOn ? new(0.1f, 0.8f, 0.2f) : new(0.1f, 0.1f, 0.1f, 0.5f);
            btnText = isOn ? "On" : "Off";
        }
    }

    //Useless items for testing
    //    [TitleGroup("Indicators")]
    //    [CustomValueDrawer("DrawColoredBool")]
    //    private bool lightOn = false;
    //#if UNITY_EDITOR // Editor-related code must be excluded from builds
    //    private static bool DrawColoredBool(bool value, GUIContent label, InspectorProperty property, Func<GUIContent, bool> callNextDrawer)
    //    {
    //        callNextDrawer(label);

    //        Rect rect = property.LastDrawnValueRect;

    //        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
    //        {
    //            value = !value;
    //            GUI.changed = true;
    //            Event.current.Use();
    //        }

    //        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new(0.1f, 0.8f, 0.2f, 0.5f) : new(0.1f, 0.1f, 0.1f, 0.5f));

    //        return value;
    //    }
    //#endif
}
