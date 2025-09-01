using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;
/// <summary>
/// Make icons blink
/// </summary>
public class Blinker : MonoBehaviour
{
    /// <summary>
    /// Target graphic
    /// </summary>
    public MaskableGraphic graphic;
    /// <summary>
    /// Blink period in seconds
    /// </summary>
    [Min(0)]
    public float period = 1f;
    /// <summary>
    /// How long the target stays at maxium brightness
    /// </summary>
    [PropertyRange(0,"period")]
    public float steadyTime = 0.5f;

    private void OnValidate()
    {
        if (graphic == null) graphic = GetComponent<MaskableGraphic>();
    }
    
    // Update is called once per frame
    void Update()
    {
        float progress = Time.timeSinceLevelLoad % period / (period - steadyTime);
        //Color temp = graphic.color;   //Use Transparency, might be laggy
        //temp.a = progress;
        //graphic.color = temp;
        graphic.color = Color.Lerp(Color.black, Color.white, progress);

    }
}
