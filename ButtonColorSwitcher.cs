using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to switch colors according to a bool. 
/// Needed to be added separately in a button's action
/// </summary>
public class ButtonColorSwitcher : MonoBehaviour
{
    //Works on image, rawimage, textfield, etc.
    public MaskableGraphic[] targetGraphics;

    public Color onCol;
    public Color offCol;

    //Change color when setting state
    private bool _state;
    public bool state
    {
        get { return this._state; }
        set
        {
            this._state = value;
            foreach (var g in targetGraphics)
            {
                if (state)
                {
                    g.color = onCol;
                }
                else
                {
                    g.color = offCol;
                }
            }
        }
    }
}
