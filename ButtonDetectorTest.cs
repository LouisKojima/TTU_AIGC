using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Non-relative tests
/// </summary>
public class ButtonDetectorTest : MonoBehaviour
{
    public Button button;
    [ShowInInspector, ReadOnly]
    private Button oldBtn;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    [Button]
    private void Reset()
    {
        button = null;
        oldBtn = null;
    }

    private void OnValidate()
    {
        if (button == null || !button.Equals(oldBtn))
        {
            BtnChanged();
        }
    }

    public void BtnChanged()
    {
        Debug.Log("btnChanged " + oldBtn?.name + "--->" +button?.name);
        //oldBtn.onClick.RemoveListener(delegate { clickDetected(button.name); });
        string str = button?.name;
        button?.onClick.AddListener(delegate { clickDetected(str); });
        oldBtn = button;

    }
    void clickDetected()
    {
        Debug.Log("clickDetected" + button.name);
    }
    void clickDetected(string name)
    {
        Debug.Log("clickDetected" + name);
    }
    void clickDetected(int n)
    {
        Debug.Log("clickDetected" + n);
    }
}

//#if UNITY_EDITOR
//[CustomEditor(typeof(ButtonDetectorTest))]
//public class ButtonDetectorTestEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        EditorGUI.BeginChangeCheck();

//        DrawPropertiesExcluding(serializedObject, "flag", "i");

//        var ButtonDetectorTest = target as ButtonDetectorTest;

//        ButtonDetectorTest.flag = GUILayout.Toggle(ButtonDetectorTest.flag, "Flag");

//        if (ButtonDetectorTest.flag)
//            ButtonDetectorTest.i = EditorGUILayout.IntSlider("I field:", ButtonDetectorTest.i, 1, 100);
//        if (EditorGUI.EndChangeCheck())
//        {
//            serializedObject.ApplyModifiedProperties();

//        }
//    }
//}
//#endif