using IAVTools;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FieldUpdater : MonoBehaviour
{
    public object target;

    //Stores Correspondencies
    [ShowInInspector]
    private List<(FieldInfo, GameObject)> fieldObjs = new();
    //Excludes
    protected string[] exludeFields = { "driveGears", "dateTime", };

    [Button]
    void Generate()
    {
        fieldObjs = transform.GenerateField(target, exludeFields);
    }

    // Update is called once per frame
    void Update()
    {
        fieldObjs.UpdateFields(target);
    }
}
