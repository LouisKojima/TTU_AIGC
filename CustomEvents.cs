using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CustomEvents : MonoBehaviour
{
    public List<UnityEvent> noParamEvents = new();
    public UnityEvent<bool> boolParam = new();
    public UnityEvent<int> intParam = new();
    public UnityEvent<float> floatParam = new();
    public UnityEvent<string> stringParam = new();

    public void TriggerNoParam(int index)
    {
        noParamEvents[index]?.Invoke();
    }

    public void TriggerBoolParam(bool param)
    {
        boolParam.Invoke(param);
    }

    public void TriggerIntParam(int param)
    {
        intParam.Invoke(param);
    }

    public void TriggerFloatParam(float param)
    {
        floatParam.Invoke(param);
    }

    public void TriggerStringParam(string param)
    {
        stringParam.Invoke(param);
    }
}
