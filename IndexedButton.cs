using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

[AddComponentMenu("UI/IndexedButton", 30)]
public class IndexedButton : Button
{
    [DisableInPlayMode]
    public IndexSource indexSource;
    public UnityEvent<int> onClickIndexed = new();

    protected override void Awake()
    {
        onClick.AddListener(() => onClickIndexed.Invoke(indexSource.getIndex()));
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor for this to show the 2 additional events in inspector
/// </summary>
[CustomEditor(typeof(IndexedButton))]
public class IndexedButtonEditor : ButtonEditor
{
    SerializedProperty m_indexSource;
    SerializedProperty m_onClickIndexedProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_indexSource = serializedObject.FindProperty("indexSource");
        m_onClickIndexedProperty = serializedObject.FindProperty("onClickIndexed");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(m_indexSource);
        EditorGUILayout.PropertyField(m_onClickIndexedProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
