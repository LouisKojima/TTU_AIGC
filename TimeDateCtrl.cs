using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using TMPro;
using System.Globalization;
/// <summary>
/// Controller for displaying time & date 
/// </summary>
[ExecuteInEditMode]
public class TimeDateCtrl : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI date;
    [ShowInInspector, DisplayAsString]
    public DateTime dateTime = System.DateTime.Now;
    [DelayedProperty]
    public string timeFormat = "";
    [DelayedProperty]
    public string dateFormat = "";

    public void updateText()
    {
        dateTime = System.DateTime.Now;
        if (time != null)
        {
            if (timeFormat != "")
            {
                time.text = dateTime.ToString(timeFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                time.text = dateTime.ToLongTimeString();
            }
        }
        if (date != null)
        {
            if (dateFormat != "")
            {
                date.text = dateTime.ToString(dateFormat, CultureInfo.InvariantCulture);
                //date.text = dateTime.ToString("dd MMMM, dddd");
            }
            else
            {
                date.text = dateTime.ToShortDateString();
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        updateText();
    }
}
