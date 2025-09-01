using IAVTools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Generates UIs according to the vehicle data and links them. 
/// </summary>
public class DevFPKFieldsCtrl : MonoBehaviour
{
    public CarControl carControl;
    [ShowInInspector]
    public VehicleStatusData target { get => carControl.data; }

    //Stores Correspondencies
    [ShowInInspector]
    private List<(FieldInfo, GameObject)> fieldObjs = new();
    //Excludes
    protected string[] exludeFields = { "driveGear", "transmission", "dateTime", "mileage", "tripStart" };

    [Button]
    // Start is called before the first frame update
    void Start()
    {
        fieldObjs = transform.GenerateField(target, exludeFields);
    }

    // Update is called once per frame
    void Update()
    {
        fieldObjs.UpdateFields(target);
    }
}
