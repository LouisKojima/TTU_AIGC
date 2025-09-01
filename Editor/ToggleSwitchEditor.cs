//using UnityEngine;
//using UnityEditor;

//namespace UnityEditor.UI
//{
///// <summary>
///// Editor for ToggleSwitch to show the 2 additional events in inspector
///// Moved to the ToggleSwitch file
///// </summary>
//    [CustomEditor(typeof(ToggleSwitch))]
//    public class ToggleSwitchEditor : ToggleEditor
//    {
//        SerializedProperty m_onPressedProperty;
//        SerializedProperty m_onReleasedProperty;

//        protected override void OnEnable()
//        {
//            base.OnEnable();

//            m_onPressedProperty = serializedObject.FindProperty("onPressed");
//            m_onReleasedProperty = serializedObject.FindProperty("onReleased");
//        }

//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();

//            serializedObject.Update();

//            EditorGUILayout.PropertyField(m_onPressedProperty);
//            EditorGUILayout.PropertyField(m_onReleasedProperty);

//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}
