using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilabData : MonoBehaviour
{
    [FoldoutGroup("Refs")]
    public VehicleStatusData vehicleData;
    [FoldoutGroup("Refs")]
    public int indexLeftSignal, indexRightSignal, indexLowBeam, indexHighBeam;
    [FoldoutGroup("Refs")]
    [ShowInInspector]
    [TableList(ShowIndexLabels = true, ShowPaging = false, DrawScrollView = true)]
    private List<VehicleStatusData.Indicator> indicators { get { return vehicleData?.toggles; } }

    public void UpdateData()
    {
        if (vehicleData == null) return;
        vehicleData.speed = speed;
        vehicleData.rpm = rotateSpeed;

        vehicleData.toggles[indexLeftSignal].isOn = isSignalLeft;
        vehicleData.toggles[indexRightSignal].isOn = isSignalRight;
        vehicleData.toggles[indexLowBeam].isOn = isLowerBeam;
        vehicleData.toggles[indexHighBeam].isOn = isHighBeam;
    }

    public bool isSignalRight = false; //右转向灯
    public bool isSignalLeft = false;//左转向灯
    public bool isHighBeam = false;//远光灯
    public bool isLowerBeam = false;//近光灯
    public float speed = 0;//速度
    public float rotateSpeed = 0;//转速

    [FoldoutGroup("Not In Use", Order = 100)]
    public int startAngleS;
    [FoldoutGroup("Not In Use")]
    public int startAngleR;
    [FoldoutGroup("Not In Use")]
    public int startValueS;
    [FoldoutGroup("Not In Use")]
    public int startValueR;
    [FoldoutGroup("Not In Use")]
    public int endValueS;
    [FoldoutGroup("Not In Use")]
    public int endValueR;
    [FoldoutGroup("Not In Use")]
    public float gear = 0;//挡位
    [FoldoutGroup("Not In Use")]
    public int p = 0;//挡位
    [FoldoutGroup("Not In Use")]
    public bool isTakeOver;
    [FoldoutGroup("Not In Use")]
    public bool isTakeOver1;
    [FoldoutGroup("Not In Use")]
    public bool isP;//20210629
    [FoldoutGroup("Not In Use")]
    public float ttcTime;
    [FoldoutGroup("Not In Use")]
    public int ttc;

    [DisplayAsString, LabelText("TCP Connected: "), GUIColor("@isConnectTcp? Color.green : Color.red")]
    public bool isConnectTcp = false;
    public static SilabData instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Init();
    }
    /// <summary>
    /// 设置初始数据
    /// </summary>
    public void Init()
    {
        if (vehicleData == null) return;
        speed = vehicleData.speed;
        rotateSpeed = vehicleData.rpm;

        isSignalLeft = vehicleData.toggles[indexLeftSignal].isOn;
        isSignalRight = vehicleData.toggles[indexRightSignal].isOn;
        isLowerBeam = vehicleData.toggles[indexLowBeam].isOn;
        isHighBeam = vehicleData.toggles[indexHighBeam].isOn;
    }
    void Update()
    {
        
    }
}
