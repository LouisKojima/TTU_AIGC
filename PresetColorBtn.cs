using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ColorCtrl;

[ExecuteInEditMode]
public class PresetColorBtn : MonoBehaviour
{
    [OnValueChanged(nameof(ApplyAutoBackground))]
    public Color color1 = Color.white;
    [OnValueChanged(nameof(ApplyAutoBackground))]
    public Color color2 = Color.black;
    [OnValueChanged(nameof(ApplyAutoBackground))]
    [EnumToggleButtons]
    public AmbientColorMode ambientColorMode = AmbientColorMode.Primary;

    public bool applyBGs = true;
    [FoldoutGroup("AutoBackgrounds")]
    public Image bgMain, bgSec, staticGradient;
    [FoldoutGroup("AutoBackgrounds")]
    public List<TMP_Text> texts;
    [FoldoutGroup("AutoBackgrounds")]
    public bool applyTextCols = false;
    [FoldoutGroup("AutoBackgrounds")]
    [Button]
    public void ApplyAutoBackground()
    {
        if (!applyBGs) return;

        bgMain.color =
            ambientColorMode == AmbientColorMode.Primary ||
            ambientColorMode == AmbientColorMode.Combined ? color1 : color2;
        bgSec.color =
            ambientColorMode == AmbientColorMode.Primary ||
            ambientColorMode == AmbientColorMode.Reverse ? color1 : color2;

        bool isSingleCol =
            ambientColorMode == AmbientColorMode.Primary ||
            ambientColorMode == AmbientColorMode.Secondary;
        //staticGradient.gameObject.SetActive(isSingleCol);
        bgSec.gameObject.SetActive(!isSingleCol);

        if (!applyTextCols) return;
        Color middleCol = isSingleCol ? bgMain.color : Color.Lerp(bgMain.color, bgSec.color, 0.5f);
        Color.RGBToHSV(middleCol, out _, out _, out float v);
        Color textCol = Color.HSVToRGB(0, 0, 1 - Mathf.Round(v + 0.1f));
        texts.ForEach(x => x.color = textCol);
    }

    [FoldoutGroup("Custom")]
    public bool isCustom = false;
    [FoldoutGroup("Custom")]
    public ColorCtrl colorCtrl;
    [FoldoutGroup("Custom")]
    public Color previousCol1, previousCol2;
    [FoldoutGroup("Custom")]
    public AmbientColorMode previousMode;

    public void AcceptCustom()
    {
        previousCol1 = color1;
        previousCol2 = color2;
        previousMode = ambientColorMode;

        ApplyAutoBackground();
    }
    public void CancelCustom()
    {
        colorCtrl.mainSlider.SetColor(previousCol1);
        colorCtrl.secondarySlider.SetColor(previousCol2);
        colorCtrl.setMode(previousMode);
    }
    private void Update()
    {
        if (isCustom)
        {
            color1 = colorCtrl.mainSlider.chosenCol;
            color2 = colorCtrl.secondarySlider.chosenCol;
            ambientColorMode = colorCtrl.mode;
            //ApplyAutoBackground();
        }
    }

    private void Start()
    {
        //if (!isCustom)
            ApplyAutoBackground();
    }

    public void ApplyPresetColor()
    {
        if (isCustom) return;
        FindObjectOfType<ColorCtrl>().UsePreset(this);
    }

    public void ApplyCustomColor()
    {
        if (isCustom) return;
        FindObjectOfType<ColorCtrl>().usePreset = false;
    }
}
