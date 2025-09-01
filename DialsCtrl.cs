using IAVTools;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Controller for dials on the FPK
/// </summary>
public class DialsCtrl : MonoBehaviour
{
    public enum DialMode { Legacy, Dial, Fill, Bar, Text };
    //Menu class for dials
    //Set color to white to be controlled by other source
    [Serializable]
    //[CustomContextMenu("Rename", "@$value.ShowRename()")]
    public class DialMenu
    {
        #region Constructors
        public DialMenu()
        {
            showRename = true;
        }
        public DialMenu(string name)
        {
            this.dialName = name;
        }
        public DialMenu(string name, DialMode mode)
        {
            this.dialName = name;
            this.dialMode = mode;
        }
        #endregion
        //
        //
        //Display Modes WIP, use Legacy for now.
        //
        //
#if UNITY_EDITOR
        //        [InfoBox("Bar Mode WIP, pls use other modes for now.", InfoMessageType.Warning, VisibleIf = nameof(isBar))]
        [BoxGroup("MainMenu")]
        [LabelText("Display Mode"), EnumPaging, PropertyOrder(-1)]
        [CustomContextMenu("Rename", nameof(ShowRename))]
#endif
        public DialMode dialMode = DialMode.Legacy;

        private bool isLagacy => dialMode == DialMode.Legacy;
        private bool isDial => dialMode == DialMode.Dial;
        private bool isFill => dialMode == DialMode.Fill;
        private bool isBar => dialMode == DialMode.Bar;
        private bool isText => dialMode == DialMode.Text;

        #region Naming nemu
        private bool showRename = false;
#if UNITY_EDITOR
        private void ShowRename() { showRename = true; }
        private void HideRename() { showRename = false; }
        [BoxGroup("MainMenu", ShowLabel = false)]
        [ShowIf(nameof(showRename)), PropertyOrder(-1)]
        [LabelText("New Name"), LabelWidth(80)]
        [InlineButton(nameof(HideRename), "Finish")]
#endif
        public string dialName;
        #endregion
        #region Main menu
#if UNITY_EDITOR
        private bool showMenu = false;
        [HorizontalGroup("MainMenu/Menu", Order = 0)]
        [Button("$" + nameof(dialName)), PropertyOrder(-1)]
        private void ShowMenu() { showMenu = !showMenu; }

        [HorizontalGroup("MainMenu/Menu"), PropertyRange(nameof(lowerValue), nameof(upperValue)), LabelWidth(50)]
        [OnValueChanged(nameof(UpdateDial))]
#endif
        public float value;

#if UNITY_EDITOR
        private bool pointerShown => isLagacy || isDial;
        //[ShowIfGroup("MainMenu/DropDown", Condition = "@(dialMode == DialMode.Legacy || dialMode == DialMode.Dial)&& showMenu", Order = 1)]
        [ShowIfGroup("MainMenu/DropDown", Condition = nameof(showMenu), Order = 1)]
        [ShowIf(nameof(pointerShown))]
#endif
        public RectTransform pointer;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown")]
        [ShowIf(nameof(isLagacy))]
        [LabelText("Tail (Optional)")]
#endif
        public RawImage tail;

#if UNITY_EDITOR
        private bool arcShown => isLagacy || isFill;
        private string arcLabel => isLagacy ? "Arc (Optional)" : "Fill Image";
        [ShowIfGroup("MainMenu/DropDown")]
        [ShowIf(nameof(arcShown))]
        [LabelText("$" + nameof(arcLabel))]
#endif
        public Image arc;

#if UNITY_EDITOR

        [ShowIfGroup("MainMenu/DropDown")]
        [ShowIf(nameof(isBar))]
#endif
        public RectTransform bar;

#if UNITY_EDITOR
        private string displayLabel => "Display" + (isText ? "" : " (Optional)");
        [ShowIfGroup("MainMenu/DropDown")]
        [LabelText("$" + nameof(displayLabel))]
#endif
        public TextMeshProUGUI display;

        [ShowIfGroup("MainMenu/DropDown")]
        [HorizontalGroup("MainMenu/DropDown/Range")]
        [LabelText("Min Value")]
        public int lowerValue = 0;
        [HorizontalGroup("MainMenu/DropDown/Range")]
        [LabelText("Max Value")]
        public int upperValue = 200;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown")]
        [HorizontalGroup("MainMenu/DropDown/Rotation")]
        [ShowIf(nameof(pointerShown))]
#endif
        public int lowerAngle = 134;

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/DropDown/Rotation")]
        [ShowIf(nameof(pointerShown))]
#endif
        public int upperAngle = -137;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown")]
        [ShowInInspector, DisplayAsString]
        [ShowIf(nameof(pointerShown))]
#endif
        private int rotationRange { get { return upperAngle - lowerAngle; } }

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown"), PropertyOrder(3)]
        [ShowInInspector, DisplayAsString]
        [ShowIf(nameof(pointerShown))]
#endif
        private float currentAngle { get { return rotationRange * (Mathf.Min(1, (value - lowerValue) / (upperValue - lowerValue))); } }

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown")]
        [HorizontalGroup("MainMenu/DropDown/Bar")]
        [ShowIf(nameof(isBar))]
#endif
        public int lowerWidth = 5;

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/DropDown/Bar")]
        [ShowIf(nameof(isBar))]
#endif
        public int upperWidth = 100;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown")]
        [ShowInInspector, DisplayAsString]
        [ShowIf(nameof(isBar))]
#endif
        private int WidthRange { get { return upperWidth - lowerWidth; } }

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/DropDown"), PropertyOrder(3)]
        [ShowInInspector, DisplayAsString]
        [ShowIf(nameof(isBar))]
#endif
        private float currentWidth { get { return WidthRange * (Mathf.Min(1, (value + lowerValue) / (upperValue - lowerValue))); } }
        #endregion
        #region Tail menu
        private bool showTail = false;
#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/Tail", Condition = "@" + nameof(isLagacy) + " && " + nameof(tail), Order = 2)]
        [HorizontalGroup("MainMenu/Tail/Menu")]
        [Button("Tail"), PropertyOrder(-1)]
#endif
        private void ShowTail() { showTail = !showTail; }

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Tail/Menu")]
        [ShowIf(nameof(tail)), Range(0, 1), LabelText("Length"), LabelWidth(50)]
        [OnValueChanged(nameof(UpdateTail))]
#endif
        public float tailLength = 0.75f;

        private int tailRadius = 256;
#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/Tail/DropDown", Condition = nameof(showTail))]
        [PropertyRange(0, nameof(tailRadius))]
        [OnValueChanged(nameof(UpdateTail))]
#endif
        public int tailWidth = 64;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/Tail/DropDown")]
        [OnValueChanged(nameof(UpdateTail))]
#endif
        public Color tailColor = Color.white;
        #endregion
        #region Arc menu
#if UNITY_EDITOR
        private bool showArc = false;
        [ShowIfGroup("MainMenu/Arc", Condition = "@" + nameof(arcShown) + " && " + nameof(arc), Order = 2)]
        [HorizontalGroup("MainMenu/Arc/Menu")]
        [Button("Arc"), PropertyOrder(-1)]
        [ShowIf(nameof(isLagacy))]
        private void ShowArc() { showArc = !showArc; }

        [HorizontalGroup("MainMenu/Arc/Menu")]
        [Range(0, 100), LabelText("Percent"), LabelWidth(50)]
        [OnValueChanged(nameof(UpdateArc))]
        [ShowIf(nameof(isLagacy))]
        [DisableIf(nameof(linkWithMain))]
#endif
        public int arcPercent = 50;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/Arc/DropDown", Condition = "@" + nameof(showArc) + " || " + nameof(isFill))]
        [OnValueChanged(nameof(UpdateArc))]
        [ShowIf(nameof(isLagacy))]
        [ToggleLeft]
#endif
        public bool linkWithMain = false;

#if UNITY_EDITOR
        [ShowIfGroup("MainMenu/Arc/DropDown")]
        [LabelText("Display (Optional)")]
#endif
        public TextMeshProUGUI arcDisplay;

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Arc/DropDown/Color")]
        [LabelText("Arc Color"), LabelWidth(60), ValueDropdown(nameof(arcUseGradientList))]
        [OnValueChanged(nameof(UpdateArc))]
#endif
        public bool arcUseGradient = false;
        private IEnumerable arcUseGradientList = new ValueDropdownList<bool>()
        {
            { "Simple", false },
            { "Gradient", true }
        };

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Arc/DropDown/Color"), ShowIf("@!" + nameof(arcUseGradient))]
        [HideLabel]
        [OnValueChanged(nameof(UpdateArc))]
#endif
        public Color arcColor = Color.green;

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Arc/DropDown/Color"), ShowIf("@" + nameof(arcUseGradient))]
        [HideLabel]
        [OnValueChanged(nameof(UpdateArc))]
#endif
        public Gradient arcGradient = new();

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Arc/DropDown/Range")]
        [LabelText("Min Fill")]
        [ShowIf(nameof(isLagacy))]
#endif
        public int lowerFill = 14;

#if UNITY_EDITOR
        [HorizontalGroup("MainMenu/Arc/DropDown/Range")]
        [LabelText("Max Fill")]
        [ShowIf(nameof(isLagacy))]
#endif
        public int upperFill = 88;
        #endregion

        public void UpdateText()
        {
            if (display)
                display.text = Mathf.RoundToInt(value) + "";
        }

        /// <summary>
        /// Update dials according to value.
        /// </summary>
        public void UpdateDial()
        {
            switch (dialMode)
            {
                case DialMode.Legacy:
                    if (pointer)
                    {
                        pointer.rotation = (Quaternion.Euler(new(0, 0, lowerAngle + currentAngle)));
                    }
                    UpdateTail();
                    UpdateText();
                    if (linkWithMain) UpdateArc();
                    break;
                case DialMode.Dial:
                    if (pointer)
                    {
                        pointer.rotation = (Quaternion.Euler(new(0, 0, lowerAngle + currentAngle)));
                    }
                    UpdateText();
                    break;
                case DialMode.Fill:
                    UpdateArc();
                    UpdateText();
                    break;
                case DialMode.Bar:
                    float width = lowerWidth + currentWidth;
                    bar.sizeDelta = new Vector2(width, bar.sizeDelta.y);
                    UpdateText();
                    break;
                case DialMode.Text:
                    UpdateText();
                    break;
                default:
                    break;
            }
        }

        [ShowIfGroup("MainMenu/Tail/DropDown")]
        public Texture2D[] tailTextures;
        private int index;
        /// <summary>
        /// Update tail texture.
        /// </summary>
        public void UpdateTail()
        {
            switch (dialMode)
            {
                case DialMode.Legacy:
                    if (tail != null)
                    {
                        if (tailTextures == null || tailTextures.Length != upperValue - lowerValue + 1)
                        {
                            tailTextures = new Texture2D[upperValue - lowerValue + 1];
                        }
                        index = Mathf.Clamp(Mathf.CeilToInt(value), lowerValue, upperValue);
                        if (tailTextures[index] == null)
                        {
                            tailTextures[index] = CircularFormatter.GenerateArcTexture(
                                tailRadius,
                                tailWidth,
                                Mathf.FloorToInt(Mathf.Abs(currentAngle) * tailLength),
                                tailColor);
                        }
                        tail.texture = tailTextures[index];
                    }
                    break;
                case DialMode.Dial:
                    break;
                case DialMode.Fill:
                    break;
                case DialMode.Bar:
                    break;
                case DialMode.Text:
                    break;
                default:
                    break;
            }
        }

        public void GenerateTail()
        {
            switch (dialMode)
            {
                case DialMode.Legacy:
                    if (tail != null)
                    {
                        if (tailTextures == null || tailTextures.Length != upperValue - lowerValue + 1)
                        {
                            tailTextures = new Texture2D[upperValue - lowerValue + 1];
                        }
                        for (int i = 0; i < tailTextures.Length; i++)
                        {
                            if (tailTextures[i] == null)
                            {
                                tailTextures[i] = CircularFormatter.GenerateArcTexture(
                                    tailRadius,
                                    tailWidth,
                                    Mathf.FloorToInt(Mathf.Abs(Mathf.Lerp(0, rotationRange, (float)i / tailTextures.Length)) * tailLength),
                                    tailColor);
                            }
                        }
                    }
                    break;
                case DialMode.Dial:
                    break;
                case DialMode.Fill:
                    break;
                case DialMode.Bar:
                    break;
                case DialMode.Text:
                    break;
                default:
                    break;
            }
        }

        public void RegenerateTail()
        {
            switch (dialMode)
            {
                case DialMode.Legacy:
                    if (tail != null)
                    {
                        tailTextures = new Texture2D[upperValue - lowerValue + 1];
                        GenerateTail();
                    }
                    break;
                case DialMode.Dial:
                    break;
                case DialMode.Fill:
                    break;
                case DialMode.Bar:
                    break;
                case DialMode.Text:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Update arc according to fill amount.
        /// </summary>
        public void UpdateArc()
        {
            switch (dialMode)
            {
                case DialMode.Legacy:
                    if (arc)
                    {
                        if (linkWithMain) 
                            arcPercent = Mathf.RoundToInt((value - lowerValue) / (upperValue - lowerValue) * 100f);
                        
                        arc.fillAmount = Mathf.Lerp(lowerFill, upperFill, arcPercent / 100f) / 100f;

                        if (arcUseGradient)
                            arc.color = arcGradient.Evaluate(arcPercent / 100f);
                        else
                            arc.color = arcColor;

                        if (arcDisplay)
                            arcDisplay.text = arcPercent + "";
                    }
                    break;
                case DialMode.Dial:
                    break;
                case DialMode.Fill:
                    if (arc)
                    {
                        int percent = Mathf.RoundToInt((value - lowerValue) / (upperValue - lowerValue) * 100f);
                        arc.fillAmount = Mathf.Lerp(lowerFill, upperFill, percent / 100f) / 100f;

                        if (arcUseGradient)
                            arc.color = arcGradient.Evaluate(percent / 100f);
                        else
                            arc.color = arcColor;

                        if (arcDisplay)
                            arcDisplay.text = percent + "";
                    }
                    break;
                case DialMode.Bar:
                    break;
                case DialMode.Text:
                    break;
                default:
                    break;
            }
        }
    }

    //[InfoBox("Display Modes WIP, pls use Legacy mode for now.", InfoMessageType.Warning, VisibleIf = "@!legacy")]
    public bool legacy;
    [ShowIfGroup("legacy")]
    [HideLabel]
    public DialMenu leftDial = new("Left Dial", DialMode.Legacy);
    [ShowIfGroup("legacy")]
    [HideLabel]
    public DialMenu rightDial = new("Right Dial", DialMode.Legacy);

    [HideIf("legacy")]
    public List<DialMenu> dials = new();

    // Update is called once per frame
    void Update()
    {
        UpdateDials();
        UpdateArcs();
    }

    private void Start()
    {
        GenerateTails();
    }

    public void UpdateDials()
    {
        if (legacy)
        {
            leftDial.UpdateDial();
            rightDial.UpdateDial();
        }
        else
        {
            dials.ForEach(x => x.UpdateDial());
        }
    }

    public void UpdateArcs()
    {
        if (legacy)
        {
            leftDial.UpdateArc();
            rightDial.UpdateArc();
        }
        else
        {
            dials.ForEach(x => x.UpdateArc());
        }
    }

    [Button]
    public void GenerateTails()
    {
        if (legacy)
        {
            leftDial.GenerateTail();
            rightDial.GenerateTail();
        }
        else
        {
            dials.ForEach(x => x.GenerateTail());
        }
    }

    [Button]
    public void RegererateTails()
    {
        if (legacy)
        {
            leftDial.RegenerateTail();
            rightDial.RegenerateTail();
        }
        else
        {
            dials.ForEach(x => x.RegenerateTail());
        }
    }
}
