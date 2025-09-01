using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MileageCtrl : MonoBehaviour
{
    public TextMeshProUGUI mileageDisplay;
    public TextMeshProUGUI tripDisplay;

    [OnValueChanged("UpdateText")]
    public int mileage;
    [OnValueChanged("UpdateText")]
    public int trip;

    public void UpdateText()
    {        
        mileageDisplay.text = mileage.ToString("#,0");
        tripDisplay.text = trip.ToString("#,0");
    }

    private void Update()
    {
        UpdateText();
    }
}
