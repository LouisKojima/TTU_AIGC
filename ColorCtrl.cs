using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class ColorCtrl : MonoBehaviour, IEventSystemHandler
{
    public enum AmbientColorMode { Primary, Combined, Reverse, Secondary }
    [EnumToggleButtons]
    public AmbientColorMode mode = AmbientColorMode.Combined;

    [ShowInInspector]
    public bool isOn { get; set; }

    public void TurnOn()
    {
        isOn = true;
    }
    public void TurnOff()
    {
        isOn = false;
    }
    public void ToggleOnOff()
    {
        isOn = !isOn;
    }
    public void TurnOnOff(bool toTurn)
    {
        isOn = toTurn;
    }

    [FoldoutGroup("Toggles")]
    public Toggle mainToggle;
    [FoldoutGroup("Toggles")]
    public Toggle combinedToggle;
    [FoldoutGroup("Toggles")]
    public Toggle reverseToggle;
    [FoldoutGroup("Toggles")]
    public Toggle secondaryToggle;

    public void setMode(AmbientColorMode toSet)
    {
        switch (toSet)
        {
            case AmbientColorMode.Primary:
                mainToggle.isOn = true;
                break;
            case AmbientColorMode.Combined:
                combinedToggle.isOn = true;
                break;
            case AmbientColorMode.Reverse:
                reverseToggle.isOn = true;
                break;
            case AmbientColorMode.Secondary:
                secondaryToggle.isOn = true;
                break;
            default:
                break;
        }
    }

    public List<MaskableGraphic> targetMain;
    public List<MaskableGraphic> targetSecondary;
    public List<Light> targetLightsMain;
    public List<Light> targetLightsSecondary;

    [Serializable]
    public class MaterialColorEntry
    {
        public Material material;
        public string nameOfColor = "_Color";
        public bool useBrightness = false;

        public MaterialColorEntry()
        {
            nameOfColor = "";
        }

        public MaterialColorEntry(Material material)
        {
            this.material = material;
        }

        public MaterialColorEntry(Material material, string nameOfColor)
        {
            this.material = material;
            this.nameOfColor = nameOfColor;
        }

        internal void Deconstruct(out Material material, out string nameOfColor)
        {
            material = this.material;
            nameOfColor = this.nameOfColor;
        }

        internal void Deconstruct(out Material material, out string nameOfColor, out bool useBrightness)
        {
            material = this.material;
            nameOfColor = this.nameOfColor;
            useBrightness = this.useBrightness;
        }

        public static implicit operator Material(MaterialColorEntry v)
        {
            return v.material;
        }
    }

    public List<MaterialColorEntry> targetMaterialsMain;
    public List<MaterialColorEntry> targetMaterialsSecondary;
    [InlineEditor]
    public ColorChangerSlider mainSlider;
    [InlineEditor]
    public ColorChangerSlider secondarySlider;

    //Brightness Menu
    private bool showBrightnessMenu = false;

    [BoxGroup("Brightness", ShowLabel = false)]
    [HorizontalGroup("Brightness/BrightnessTitle")]
    [VerticalGroup("Brightness/BrightnessTitle/Left", Order = 0), Button("Brightness")]
    private void BrightnessMenu() { showBrightnessMenu = !showBrightnessMenu; }
    [PropertyRange(0, 1), HideLabel]
    [VerticalGroup("Brightness/BrightnessTitle/Right", Order = 1)]
    public float brightness;

    [ShowIfGroup("Brightness/BrightnessSlider", Condition = nameof(showBrightnessMenu))]
    public Slider brightnessSlider;

    [ShowInInspector]
    public bool usePreset { get; set; }
    public PresetColorBtn preset;

    public Color mainColor;
    public Color mainColorWithBrightness;
    public Color secondaryColor;
    public Color secondaryColorWithBrightness;

    public void UsePreset(PresetColorBtn param)
    {
        usePreset = true;
        preset = param;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    [OnInspectorGUI]
    void Update()
    {
        if (mainToggle.isOn) mode = AmbientColorMode.Primary;
        else if (combinedToggle.isOn) mode = AmbientColorMode.Combined;
        else if (reverseToggle.isOn) mode = AmbientColorMode.Reverse;
        else if (secondaryToggle.isOn) mode = AmbientColorMode.Secondary;

        if (isOn)
        {
            CalculateColors();
        }
        else
        {
            mainColor = Color.black;
            mainColor.a = 0;
            secondaryColor = Color.black;
            secondaryColor.a = 0;
            mainColorWithBrightness = Color.black;
            secondaryColorWithBrightness = Color.black;
        }

        ApplyColors();
    }

    private void CalculateColors()
    {
        if (usePreset)
        {
            mainColor =
                preset.ambientColorMode == AmbientColorMode.Primary ||
                preset.ambientColorMode == AmbientColorMode.Combined
                ? preset.color1 : preset.color2;
            secondaryColor =
                preset.ambientColorMode == AmbientColorMode.Primary ||
                preset.ambientColorMode == AmbientColorMode.Reverse
                ? preset.color1 : preset.color2;
        }
        else
        {
            mainColor = mainToggle.isOn || combinedToggle.isOn ? mainSlider.chosenCol : secondarySlider.chosenCol;
            secondaryColor = mainToggle.isOn || reverseToggle.isOn ? mainSlider.chosenCol : secondarySlider.chosenCol;
        }

        brightness = brightnessSlider.normalizedValue;

        Color.RGBToHSV(mainColor, out float hm, out float sm, out float _);
        mainColorWithBrightness = Color.HSVToRGB(hm, sm, brightness);
        Color.RGBToHSV(secondaryColor, out float hs, out float ss, out float _);
        secondaryColorWithBrightness = Color.HSVToRGB(hs, ss, brightness);
    }

    private void ApplyColors()
    {
        foreach (var g in targetMain)
        {
            g.color = mainColor;
        }
        foreach (var g in targetSecondary)
        {
            g.color = secondaryColor;
        }
        foreach (var l in targetLightsMain)
        {
            l.color = mainColorWithBrightness;
        }
        foreach (var l in targetLightsSecondary)
        {
            l.color = secondaryColorWithBrightness;
        }
        foreach ((Material m, string n, bool b) in targetMaterialsMain)
        {
            m.SetColor(n, b ? mainColorWithBrightness : mainColor);
        }
        foreach ((Material m, string n, bool b) in targetMaterialsSecondary)
        {
            m.SetColor(n, b ? secondaryColorWithBrightness : secondaryColor);
        }
    }

    private void OnValidate()
    {
        ApplyBrightnessToSlider();
    }

    private void ApplyBrightnessToSlider()
    {
        brightnessSlider.normalizedValue = brightness;
    }
}
