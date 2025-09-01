using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
#endif

[CreateAssetMenu(fileName = "TestScriptableObject.asset", menuName = "IAV/TestScriptableObject", order = 1)]
[Serializable]
public class TestScriptableObject : ScriptableObject
{
    public GameObject gameObject;

    public void SomeFunction()
    {

    }
    public void SomeFunction(int i)
    {

    }
    public void SomeFunction(string s)
    {

    }
}
