using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Car Controller handling all data
/// </summary>
[HelpURL("https://confluence.iavgroup.local/display/CNTTU/CarCtrl")]
[ExecuteInEditMode]
public class CarControl : MonoBehaviour
{
    //Directly edit the data, shown at the bottom
    [InlineEditor(InlineEditorObjectFieldModes.Hidden), PropertyOrder(100)]
    public VehicleStatusData data;

    //Controls for components
    [FoldoutGroup("Controls")]
    public DialsCtrl[] dialsCtrls;
    [FoldoutGroup("Controls")]
    public GearsCtrl[] gearsCtrls;
    [FoldoutGroup("Controls")]
    public TimeDateCtrl[] timeDateCtrls;
    [FoldoutGroup("Controls")]
    public IndicatorsCtrl[] indicatorsCtrls;
    [FoldoutGroup("Controls")]
    public TextMeshProUGUI[] temperatures;
    [FoldoutGroup("Controls")]
    public MileageCtrl[] mileageCtrls;
    [FoldoutGroup("Controls")]
    public Toggle FPKSwitch;

    [DelayedProperty]
    public float rpmRatio { get => _rpmRatio; set => _rpmRatio = value; }
    private float _rpmRatio = 100f;

    //Locks individual component controls only when shown
    private bool isShown;
    [OnInspectorInit]
    private void OnShow() { isShown = true; }
    [OnInspectorDispose]
    private void OnHide() { isShown = false; }

    private void Start()
    {
    }

    private bool changed = false;
    public void Update()
    {
        if (isShown || Application.isPlaying)
        {
            if (data != null )
            {
                data.dateTime = System.DateTime.Now;
                if (FPKSwitch.isOn != data.newFPK)
                    FPKSwitch.isOn = data.newFPK;
                //Dials
                foreach (DialsCtrl dialsCtrl in dialsCtrls)
                {
                    changed = false;
                    if (dialsCtrl == null) continue;
                    if (dialsCtrl.legacy)
                    {
                        if (changed |= dialsCtrl.leftDial.value != data.rpm) 
                            dialsCtrl.leftDial.value = data.rpm / rpmRatio;
                        if (changed |= dialsCtrl.rightDial.value != Mathf.Abs(data.speed)) 
                            dialsCtrl.rightDial.value = Mathf.Abs(data.speed);
                        if (changed) 
                            dialsCtrl.UpdateDials();
                        changed = false;

                        if (changed = dialsCtrl.leftDial.arcPercent != data.batteryPercent) 
                            dialsCtrl.leftDial.arcPercent = data.batteryPercent;
                        if (changed) 
                            dialsCtrl.UpdateArcs();
                        changed = false;

                        if (changed = dialsCtrl.rightDial.arcPercent != data.boostPercent) 
                            dialsCtrl.rightDial.arcPercent = data.boostPercent;
                        if (changed) 
                            dialsCtrl.UpdateArcs();
                        changed = false;
                    }
                    else
                    {
                        if (changed |= dialsCtrl.dials[1].value != data.rpm)
                            dialsCtrl.dials[1].value = data.rpm / rpmRatio;
                        if (changed |= dialsCtrl.dials[0].value != Mathf.Abs(data.speed))
                            dialsCtrl.dials[0].value = Mathf.Abs(data.speed);
                        if (changed = dialsCtrl.dials[2].value != data.batteryPercent)
                            dialsCtrl.dials[2].value = data.batteryPercent;
                        if (changed = dialsCtrl.dials[3].value != data.boostPercent)
                            dialsCtrl.dials[3].value = data.boostPercent;
                        if (changed)
                            dialsCtrl.UpdateDials();
                        changed = false;
                    }
                }
                //PRND
                foreach (GearsCtrl gearsCtrl in gearsCtrls)
                {
                    changed = false;
                    if (gearsCtrl != null)
                    {
                        if (changed |= gearsCtrl.transmission != data.transmission)
                            gearsCtrl.transmission = data.transmission;
                        if (changed |= gearsCtrl.number != data.driveGear)
                            gearsCtrl.number = data.driveGear;
                        if (changed) 
                            gearsCtrl.UpdateGears();
                        changed = false;
                    }
                }
                //Time (not in use)
                //foreach (TimeDateCtrl timeDateCtrl in timeDateCtrls)
                //{
                //    if (timeDateCtrl != null)
                //    {
                //        timeDateCtrl.dateTime = statusData.dateTime;
                //        timeDateCtrl.updateText();
                //    }
                //}
                //Indicators are set according to their indices in list
                foreach (IndicatorsCtrl indicatorsCtrl in indicatorsCtrls)
                {
                    if (indicatorsCtrl != null)
                    {
                        for(int i = 0; i < data.toggles.Count; i++)
                        {
                            indicatorsCtrl.SetIndicator(i, data.toggles[i].isOn);
                        }//Set indicators
                    }
                }
                //Temperature
                foreach (var temperature in temperatures)
                {
                    changed = false;
                    if (temperature != null)
                    {
                        if (changed = !temperature.text.Equals(data.temperature + (data.useC ? "¡ãC" : "¡ãF"))) 
                            temperature.text = data.temperature + (data.useC ? "¡ãC" : "¡ãF");
                        changed = false;
                    }
                }
                foreach (MileageCtrl mileageCtrl in mileageCtrls)
                {
                    changed = false;
                    if (mileageCtrl != null)
                    {
                        if (changed |= mileageCtrl.mileage != data.mileage) 
                            mileageCtrl.mileage = data.mileage;
                        if (changed |= mileageCtrl.trip != data.mileage - data.tripStart) 
                            mileageCtrl.trip = data.mileage - data.tripStart;
                        if (changed) 
                            mileageCtrl.UpdateText();
                        changed = false;
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        Update();
    }
}
