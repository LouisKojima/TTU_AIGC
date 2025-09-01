using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Using silders to change colors.
/// </summary>
public class ColorChangerSlider : MonoBehaviour
{
    public string description;
    //public Color[] colors;

    //Graphics Menu
    //Add graphics to the list to change their colors
    //Also support group actions in second tab
    [TabGroup("TargetGraphics")]
    public List<MaskableGraphic> targetGraphics;
    [TabGroup("GroupSelection")]
    public GameObject targetGroup;
    [TabGroup("GroupSelection")]
    [InlineButton(nameof(RemoveFromGraphics), "Delete"),InlineButton(nameof(SaveToGraphics), "Add"), InlineButton(nameof(FindTargetGraphics), "Find")]
    public string keyWord = "";
    [TabGroup("GroupSelection"), HideIf("@this.targetsFound.Count == 0")]
    public List<MaskableGraphic> targetsFound;

    private void FindTargetGraphics()
    {
        if (targetGroup != null)
        {
            targetsFound = targetGroup.GetComponentsInChildren<MaskableGraphic>(true)
                .Where(t => t.name.ToLower().Contains(keyWord.ToLower())).ToList();
        }
        else
        {
            targetsFound.Clear();
        }
    }
    private void SaveToGraphics()
    {
        targetGraphics.AddRange(targetsFound);
    }
    private void RemoveFromGraphics()
    {
        targetGraphics.RemoveAll(t => targetsFound.Contains(t));
    }

    //Color Menu
    private bool showColorMenu = false;

    [BoxGroup("Color", ShowLabel = false)]
    [HorizontalGroup("Color/ColorTitle")]
    [VerticalGroup("Color/ColorTitle/Left", Order = 0), Button("Color")]
    private void ColorMenu() { showColorMenu = !showColorMenu; }

    [PropertyRange(0, 1), HideLabel, GUIColor("@Color.HSVToRGB(color, saturation, brightness)")]
    [VerticalGroup("Color/ColorTitle/Right", Order = 1)]
    public float color;

    private bool isColorSliderNull = true;

    [OnValueChanged(nameof(FindColorSlider))]
    [ShowIfGroup("Color/ColorSlider", Condition = "showColorMenu")]
    public Slider colorSlider;

    [ShowIfGroup("Color/ColorSlider", Condition = "showColorMenu")]
    [HideIfGroup("Color/ColorSlider/isColorSliderNull")]
    public RawImage colorSliderBG;
    
    public void FindColorSlider()
    {
        isColorSliderNull = colorSlider == null;
        colorSliderBG = colorSlider?.GetComponentInChildren<RawImage>();
    }

    //Saturation Menu
    private bool showSaturationMenu = false;

    [BoxGroup("Saturation", ShowLabel = false)]
    [HorizontalGroup("Saturation/SaturationTitle")]
    [VerticalGroup("Saturation/SaturationTitle/Left", Order = 0), Button("Saturation")]
    private void SaturationMenu() { showSaturationMenu = !showSaturationMenu; }

    [PropertyRange(0, 1), HideLabel, GUIColor("@Color.HSVToRGB(color, saturation, brightness)")]
    [VerticalGroup("Saturation/SaturationTitle/Right", Order = 1)]
    public float saturation;

    private bool isSaturationSliderNull = true;

    [OnValueChanged(nameof(FindSaturationSlider))]
    [ShowIfGroup("Saturation/SaturationSlider", Condition = "showSaturationMenu")]
    public Slider saturationSlider;

    [ShowIfGroup("Saturation/SaturationSlider", Condition = "showSaturationMenu")]
    [HideIfGroup("Saturation/SaturationSlider/isSaturationSliderNull")]
    public RawImage saturationSliderBG;
    public void FindSaturationSlider()
    {
        isSaturationSliderNull = saturationSlider == null;
        saturationSliderBG = saturationSlider?.GetComponentInChildren<RawImage>();
    }

    //Brightness Menu
    private bool showBrightnessMenu = false;

    [BoxGroup("Brightness", ShowLabel = false)]
    [HorizontalGroup("Brightness/BrightnessTitle")]
    [VerticalGroup("Brightness/BrightnessTitle/Left", Order = 0), Button("Brightness")]
    private void BrightnessMenu() { showBrightnessMenu = !showBrightnessMenu; }

    [PropertyRange(0, 1), HideLabel, GUIColor("@Color.HSVToRGB(color, saturation, brightness)")]
    [VerticalGroup("Brightness/BrightnessTitle/Right", Order = 1)]
    public float brightness;

    private bool isBrightnessSliderNull = true;

    [OnValueChanged(nameof(FindBrightnessSlider))]
    [ShowIfGroup("Brightness/BrightnessSlider", Condition = "showBrightnessMenu")]
    public Slider brightnessSlider;

    [ShowIfGroup("Brightness/BrightnessSlider", Condition = "showBrightnessMenu")]
    [HideIfGroup("Brightness/BrightnessSlider/isBrightnessSliderNull")]
    public RawImage brightnessSliderBG;
    public void FindBrightnessSlider()
    {
        isBrightnessSliderNull = brightnessSlider == null;
        brightnessSliderBG = brightnessSlider?.GetComponentInChildren<RawImage>();
    }

    private int textureWidth = 128;
    public static int textureHeight = 10;

    private void UpdateSliderBG(RawImage sliderBG, HSV hsv = HSV.H)
    {
        if (sliderBG.gameObject.activeInHierarchy) sliderBG.texture = GenerateTexture(hsv);
    }

    private void Reset()
    {
        Start();
    }

    // Start is called before the first frame update
    public void Start()
    {
        FindColorSlider();FindSaturationSlider();FindBrightnessSlider();
        //if (!isColorSliderNull) color = colorSlider.normalizedValue;
        //if (!isSaturationSliderNull) saturation = saturationSlider.normalizedValue;
        //if (!isBrightnessSliderNull) brightness = brightnessSlider.normalizedValue;
        //image = GetComponentInChildren<RawImage>();
        //slider = GetComponentInChildren<Slider>();
        //Generate textures for sliders
        if (!isColorSliderNull) colorSliderBG.texture = GenerateTexture();
        if (!isSaturationSliderNull) UpdateSliderBG(saturationSliderBG, HSV.S);
        if (!isBrightnessSliderNull) UpdateSliderBG(brightnessSliderBG, HSV.V);
    }

    private void OnValidate()
    {
        Start();
        ApplyColorToSlider();
        Update();
    }

    private void ApplyColorToSlider()
    {
        if (!isColorSliderNull) colorSlider.normalizedValue = color;
        if (!isSaturationSliderNull) saturationSlider.normalizedValue = saturation;
        if (!isBrightnessSliderNull) brightnessSlider.normalizedValue = brightness;
    }

    public void SetColor(float color, float saturation = 1, float brightness = 1)
    {
        if (colorSlider)
            this.color = color;
        if(saturationSlider)
            this.saturation = saturation;
        if(brightnessSlider)
            this.brightness = brightness;
        ApplyColorToSlider();
        Update();
    }

    public void SetColor(Color toSet)
    {
        Color.RGBToHSV(toSet, out float h, out float s, out float v);
        SetColor(h, s, v);
    }

    public Color chosenCol = new Color();

    public Color GetColor(float color, float saturation = 1, float brightness = 1)
    {
        return Color.HSVToRGB(Mathf.RoundToInt(color * textureWidth), saturation, brightness); 
    }

    // Update is called once per frame
    public void Update()
    {
        //Get values from sliders
        if (!isColorSliderNull) color = colorSlider.normalizedValue;
        if (!isSaturationSliderNull)
        {
            saturation = saturationSlider.normalizedValue;
            UpdateSliderBG(saturationSliderBG, HSV.S);
        }
        if (!isBrightnessSliderNull)
        {
            brightness = brightnessSlider.normalizedValue;
            UpdateSliderBG(brightnessSliderBG, HSV.V);
        }

        var colorTexture = GenerateTexture();
        //Texture2D colorTexture = (Texture2D)colorSliderBG.texture;

        //Change color of the targets
        chosenCol = colorTexture.GetPixel(Mathf.RoundToInt(colorSlider.normalizedValue * textureWidth), 0);
     
        foreach (var g in targetGraphics)
        {
            g.color = chosenCol;
        }

        //slider.handleRect.GetComponent<Image>().color = chosenCol; 
    }

    private enum HSV { H, S, V }
    /// <summary>
    /// Gererate texture for slider, varying only on H, S, or V
    /// </summary>
    /// <param name="option">HSV</param>
    /// <returns>Generated Texture</returns>
    private Texture2D GenerateTexture(HSV option = HSV.H)
    {
        var texture = new Texture2D(textureWidth, textureHeight);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        for (int h = 0; h < textureWidth; h++)
        {
            Color[] colors = new Color[textureHeight];
            switch(option)
            {
                case HSV.S:
                    Array.Fill(colors, Color.HSVToRGB(color, (float)h / textureWidth, brightness));
                    break;
                case HSV.V:
                    Array.Fill(colors, Color.HSVToRGB(color, saturation, (float)h / textureWidth));
                    break;
                case HSV.H:
                default:
                    Array.Fill(colors, Color.HSVToRGB((float)h / textureWidth, saturation, brightness));
                    break;
            }
            texture.SetPixels(h, 0, 1, textureHeight, colors);
        }
        texture.Apply();
        return texture;
    }

}
