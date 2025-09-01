using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimData : MonoBehaviour
{
    public RCC_DashboardInputs simCtrlSource;
    private RCC_CarControllerV3 simCtrl { get => simCtrlSource.carController; }
    public CarControl carCtrl;
    private VehicleStatusData vehicleData { get => carCtrl.data; }

    private List<string> nameList { get => vehicleData.toggles[0].nameList; }
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject ABSIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject ESPIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject breakIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject parkIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject lowBeamIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject highBeamIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject leftTurnIcon;
    [FoldoutGroup("Indicator Refs", expanded: true)]
    public GameObject rightTurnIcon;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (simCtrl)
        {
            vehicleData.speed = simCtrl.speed;
            vehicleData.rpm = simCtrl.engineRPM;

            if (!simCtrl.engineRunning && (simCtrl._gasInput == 0 && simCtrl.speed < 1f))
            {
                vehicleData.transmission = VehicleStatusData.Transmission.P;
            }
            else if (!simCtrl.engineRunning || simCtrl.NGear || (simCtrl._gasInput == 0 && simCtrl.speed < 1f))
            {
                vehicleData.transmission = VehicleStatusData.Transmission.N;
            }
            else if (simCtrl.direction < 0)
            {
                vehicleData.transmission = VehicleStatusData.Transmission.R;
            }
            else
            {
                vehicleData.transmission = VehicleStatusData.Transmission.D;
                vehicleData.driveGear = simCtrl.currentGear;
            }

            vehicleData.boostPercent = Mathf.RoundToInt(simCtrl.NoS);

            vehicleData.toggles.ForEach((x) =>
            {
                if (ABSIcon && x.name.Equals(ABSIcon.name))
                {
                    x.isOn = simCtrl.ABSAct;
                }
                else if (ESPIcon && x.name.Equals(ESPIcon.name))
                {
                    x.isOn = simCtrl.ESPAct;
                }
                else if (breakIcon && x.name.Equals(breakIcon.name))
                {
                    x.isOn = simCtrl.handbrakeInput > .1f;
                }
                else if (parkIcon && x.name.Equals(parkIcon.name))
                {
                    x.isOn = vehicleData.transmission == VehicleStatusData.Transmission.P;
                }
                else if (lowBeamIcon && x.name.Equals(lowBeamIcon.name))
                {
                    x.isOn = simCtrl.lowBeamHeadLightsOn;
                }
                else if (highBeamIcon && x.name.Equals(highBeamIcon.name))
                {
                    x.isOn = simCtrl.highBeamHeadLightsOn;
                }
                else if (leftTurnIcon && x.name.Equals(leftTurnIcon.name))
                {
                    x.isOn =
                    simCtrl.indicatorsOn == RCC_CarControllerV3.IndicatorsOn.All ||
                    simCtrl.indicatorsOn == RCC_CarControllerV3.IndicatorsOn.Left;
                }
                else if (rightTurnIcon && x.name.Equals(rightTurnIcon.name))
                {
                    x.isOn =
                    simCtrl.indicatorsOn == RCC_CarControllerV3.IndicatorsOn.All ||
                    simCtrl.indicatorsOn == RCC_CarControllerV3.IndicatorsOn.Right;
                }
            });
        }
    }
}
