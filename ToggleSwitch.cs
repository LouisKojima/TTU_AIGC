using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

/// <summary>
/// A toggle that also has tiggers for click events
/// </summary>
[AddComponentMenu("UI/ToggleSwitch", 30)]
public class ToggleSwitch :
    Toggle,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public UnityEvent<float> onValueChanged01 = new();
    public UnityEvent onPressed = new UnityEvent();
    public UnityEvent onReleased = new UnityEvent();
    public UnityEvent onValueOn = new UnityEvent();
    public UnityEvent onValueOff = new UnityEvent();

    protected override void Awake()
    {
        onValueChanged.AddListener((f) => { onValueChanged01.Invoke(f ? 1 : 0); });
        onValueChanged.AddListener((f) => { if (f) onValueOn.Invoke(); });
        onValueChanged.AddListener((f) => { if (!f) onValueOff.Invoke(); });
    }

    protected override void Start()
    {
        if (isOn) onValueOn.Invoke();
        else onValueOff.Invoke();
        onValueChanged.Invoke(isOn);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        onPressed.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        onReleased.Invoke();
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor for this to show the 2 additional events in inspector
/// </summary>
[CustomEditor(typeof(ToggleSwitch))]
public class ToggleSwitchEditor : ToggleEditor
{
    SerializedProperty m_onPressedProperty;
    SerializedProperty m_onReleasedProperty;
    SerializedProperty m_onValueChanged01Property;
    SerializedProperty m_onValueOnProperty;
    SerializedProperty m_onValueOffProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_onValueChanged01Property = serializedObject.FindProperty("onValueChanged01");
        m_onPressedProperty = serializedObject.FindProperty("onPressed");
        m_onReleasedProperty = serializedObject.FindProperty("onReleased");
        m_onValueOnProperty = serializedObject.FindProperty("onValueOn");
        m_onValueOffProperty = serializedObject.FindProperty("onValueOff");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(m_onValueChanged01Property);
        EditorGUILayout.PropertyField(m_onPressedProperty);
        EditorGUILayout.PropertyField(m_onReleasedProperty);
        EditorGUILayout.PropertyField(m_onValueOnProperty);
        EditorGUILayout.PropertyField(m_onValueOffProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif