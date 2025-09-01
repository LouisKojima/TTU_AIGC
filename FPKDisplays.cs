using IAVTools;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPKDisplays
{
    public enum DisplayModes { Dial, Fill, Bar, Text };

    public class DisplayMenu
    {
        private string name;
        public DisplayMenu(string name)
        {
            this.name = name;
        }

#region Main menu
        private bool showMenu = false;
        [BoxGroup("MainMenu", ShowLabel = false)]
        [HorizontalGroup("MainMenu/Menu", Order = 0)]
        [Button("$dialName"), PropertyOrder(-1)]
        private void ShowMenu() { showMenu = !showMenu; }

        [HorizontalGroup("MainMenu/Menu"), PropertyRange("lowerValue", "upperValue"), LabelWidth(50)]
        [OnValueChanged("updateDial")]
        public float value;

        [ShowIfGroup("MainMenu/DropDown", Condition = "showMenu", Order = 1)]
        [LabelText("Display Mode"), EnumPaging]
        public DisplayModes displayMode = DisplayModes.Dial;

        [ShowIfGroup("MainMenu/DropDown")]
        [LabelText("Pointer")]
        public RectTransform pointer;

        [ShowIfGroup("MainMenu/DropDown")]
        [LabelText("Tail (Optional)")]
        public RawImage tail;

        [ShowIfGroup("MainMenu/DropDown")]
        [LabelText("Arc (Optional)")]
        public Image arc;

        [ShowIfGroup("MainMenu/DropDown")]
        [LabelText("Display (Optional)")]
        public TextMeshProUGUI mainDisplay;

        [ShowIfGroup("MainMenu/DropDown")]
        [HorizontalGroup("MainMenu/DropDown/Range")]
        [LabelText("Min Value")]
        public int lowerValue = 0;
        [HorizontalGroup("MainMenu/DropDown/Range")]
        [LabelText("Max Value")]
        public int upperValue = 200;

        [ShowIfGroup("MainMenu/DropDown")]
        [HorizontalGroup("MainMenu/DropDown/Rotation")]
        [LabelText("Lower Angle")]
        public int lowerAngle = 134;
        [HorizontalGroup("MainMenu/DropDown/Rotation")]
        [LabelText("Upper Angle")]
        public int upperAngle = -137;

        [ShowIfGroup("MainMenu/DropDown")]
        [ShowInInspector, DisplayAsString, LabelText("Rotation Range")]
        private int rotationRange { get { return upperAngle - lowerAngle; } }

        [BoxGroup("MainMenu"), PropertyOrder(3)]
        [ShowInInspector, LabelText("Current Angle")]
        private float currentAngel { get { return rotationRange * (Mathf.Min(1, (value - lowerValue) / (upperValue - lowerValue))); } }
#endregion
#region Tail menu
        private bool showTail = false;
        [ShowIfGroup("MainMenu/Tail", Condition = "@tail != null", Order = 2)]
        [HorizontalGroup("MainMenu/Tail/Menu")]
        [Button("Tail"), PropertyOrder(-1)]
        private void ShowTail() { showTail = !showTail; }

        [HorizontalGroup("MainMenu/Tail/Menu")]
        [ShowIf("@tail != null"), Range(0, 1), LabelText("Length"), LabelWidth(50)]
        [OnValueChanged("updateTail")]
        public float tailLength = 0.75f;

        private int tailRadius = 256;
        [ShowIfGroup("MainMenu/Tail/DropDown", Condition = "showTail")]
        [PropertyRange(0, "tailRadius")]
        [OnValueChanged("updateTail")]
        public int tailWidth = 64;

        [ShowIfGroup("MainMenu/Tail/DropDown", Condition = "showTail")]
        [OnValueChanged("updateTail")]
        public Color tailColor = Color.cyan;
#endregion
#region Arc menu
        private bool showArc = false;
        [ShowIfGroup("MainMenu/Arc", Condition = "@arc != null || displayMode == DisplayModes.Fill", Order = 2)]
        [HorizontalGroup("MainMenu/Arc/Menu")]
        [Button("Arc"), PropertyOrder(-1)]
        private void ShowArc() { showArc = !showArc; }

        [HorizontalGroup("MainMenu/Arc/Menu")]
        [Range(0, 100), LabelText("Percent"), LabelWidth(50)]
        [OnValueChanged("updateArc")]
        public int arcPercent = 50;

        [ShowIfGroup("MainMenu/Arc/DropDown", Condition = "showArc")]
        [LabelText("Display (Optional)")]
        public TextMeshProUGUI arcDisplay;

        [HorizontalGroup("MainMenu/Arc/DropDown/Color")]
        [LabelText("Arc Color"), LabelWidth(60), ValueDropdown("arcUseGradientList")]
        [OnValueChanged("updateArc")]
        public bool arcUseGradient = false;
        private IEnumerable arcUseGradientList = new ValueDropdownList<bool>()
        {
            { "Simple", false },
            { "Gradient", true }
        };

        [HorizontalGroup("MainMenu/Arc/DropDown/Color"), ShowIf("@!arcUseGradient")]
        [HideLabel]
        [OnValueChanged("updateArc")]
        public Color arcColor = Color.green;

        [HorizontalGroup("MainMenu/Arc/DropDown/Color"), ShowIf("@arcUseGradient")]
        [HideLabel]
        [OnValueChanged("updateArc")]
        public Gradient arcGradient = new();

        [HorizontalGroup("MainMenu/Arc/DropDown/Range")]
        [LabelText("Min Value")]
        public int lowerFill = 14;

        [HorizontalGroup("MainMenu/Arc/DropDown/Range")]
        [LabelText("Max Value")]
        public int upperFill = 88;
#endregion

        /// <summary>
        /// Update dials according to value.
        /// </summary>
        public void updateDial()
        {
            if (pointer != null)
            {
                pointer.rotation = (Quaternion.Euler(new(0, 0, lowerAngle + currentAngel)));
            }
            updateTail();
            if (mainDisplay != null) mainDisplay.text = Mathf.RoundToInt(value) + "";
            //        Debug.Log(current.ToString() + "->" + targrt.ToString());
        }

        /// <summary>
        /// Regenerate tail texture.
        /// </summary>
        public void updateTail()
        {
            if (tail != null)
            {
                tail.texture = CircularFormatter.GenerateArcTexture(
                    tailRadius,
                    tailWidth,
                    Mathf.FloorToInt(Mathf.Abs(currentAngel) * tailLength),
                    tailColor);
            }
        }

        /// <summary>
        /// Update arc according to fill amount.
        /// </summary>
        public void updateArc()
        {
            if (arc != null)
            {
                arc.fillAmount = Mathf.Lerp(lowerFill, upperFill, arcPercent / 100f) / 100f;

                if (arcUseGradient) arc.color = arcGradient.Evaluate(arcPercent / 100f);
                else arc.color = arcColor;

                if (arcDisplay != null) arcDisplay.text = arcPercent + "";
            }
        }
    }

}
