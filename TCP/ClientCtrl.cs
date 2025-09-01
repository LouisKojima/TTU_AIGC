using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static VehicleStatusData;

public class ClientCtrl : MonoBehaviour
{
    public Client client;
    [Serializable]
    public class CtrlMenuItem
    {
        public enum InputType { Text, Slider, Toggle };

        [ShowInInspector]
        public UnityEngine.Object target;
        //[ShowInInspector]
        public FieldInfo[] fields { get => target?.GetType().GetFields(); }
        //[ShowInInspector]
        public List<string> fieldsList { get => fields?.Select(x => x.Name).ToList(); }

        [ValueDropdown("fieldsList")]
        [ShowInInspector, LabelText("Target Field")]
        [OnValueChanged("UpdateInputType")]
        public string targetFieldName;
        private FieldInfo targetField { get => target?.GetType().GetField(targetFieldName); }

        public InputType inputType = InputType.Text;

        private void UpdateInputType()
        {
            if (targetField == null) return;
            if (targetField.FieldType.IsPrimitive)
            {
                switch (targetField.FieldType.Name)
                {
                    case nameof(Boolean):
                        inputType = InputType.Toggle;
                        break;

                    case nameof(Int16):
                    case nameof(Int32):
                    case nameof(Int64):
                    case nameof(Single):
                    case nameof(Double):
                        inputType = InputType.Slider;
                        break;

                    default:
                        inputType = InputType.Text;
                        break;
                }
            }
            else
            {
                //Non-primitives names can be a mess, so switch type instead
                switch (targetField.FieldType)
                {

                    case var t when t == typeof(Transmission):
                        inputType = InputType.Text;
                        break;

                    default:
                        inputType = InputType.Text;
                        break;
                }
            }
        }

        public CtrlMenuItem(UnityEngine.Object target)
        {
            this.target = target;
        }
        private CtrlMenuItem() { }

        [ShowIf("@inputType == InputType.Slider")]
        public Slider slider;
        [ShowIf("@inputType == InputType.Slider || inputType == InputType.Text")]
        public InputField text;
        [ShowIf("@inputType == InputType.Toggle")]
        public Toggle toggle;

        private bool isSliderTextInSync = false;
        /// <summary>
        /// This func is now using visual scripting
        /// </summary>
        private void SyncSliderText()
        {
            //text.onValueChanged.AddListener(s => slider.SetValueWithoutNotify(float.Parse(s)));
            //slider.onValueChanged.AddListener(f => text.SetTextWithoutNotify(f + ""));
            ////Debug.Log("Sync");
            //isSliderTextInSync = true;
        }
        /// <summary>
        /// Call this once in Start()
        /// </summary>
        public void StartInit()
        {
            switch (inputType)
            {
                case InputType.Text:
                    //if (text == null) break;

                    //Type type = targetField.GetType();
                    //if (type.IsPrimitive)
                    //    targetField.SetValue(target, Convert.ChangeType(text.text, targetField.FieldType));

                    //else if (type.IsEnum)
                    //    targetField.SetValue(target, Enum.Parse(targetField.FieldType, text.text, ignoreCase: true));

                    break;
                case InputType.Slider:
                    if (slider == null) break;

                    if (!isSliderTextInSync && text != null)
                    {
                        SyncSliderText();
                        text.SetTextWithoutNotify(slider.value + "");
                    }

                    //slider.onValueChanged.AddListener(UpdateSliderValue);

                    break;
                case InputType.Toggle:
                    //if (toggle == null) break;
                    //toggle.onValueChanged.AddListener(UpdateToggleValue);
                    break;
                default:
                    break;
            }
        }

        //[Button]
        public void UpdateValue()
        {
            switch (inputType)
            {
                case InputType.Text:
                    if (text == null) break;

                    UpdateTextValue();

                    break;
                case InputType.Slider:
                    if (slider == null) break;

                    UpdateSliderValue();

                    break;
                case InputType.Toggle:
                    if (toggle == null) break;

                    UpdateToggleValue();

                    break;
                default:
                    break;
            }
        }

        private void UpdateTextValue()
        {
            Type type = targetField.GetType();
            if (type.IsPrimitive)
                targetField.SetValue(target, Convert.ChangeType(text.text, targetField.FieldType));

            else if (type.IsEnum)
                targetField.SetValue(target, Enum.Parse(targetField.FieldType, text.text, ignoreCase: true));
        }

        private void UpdateSliderValue()
        {
            targetField.SetValue(target, Convert.ChangeType(slider.value, targetField.FieldType));
        }

        private void UpdateToggleValue()
        {
            targetField.SetValue(target, toggle.isOn);
            //toggle.targetGraphic.color = toggle.isOn ? Color.green : Color.red;
        }
    }
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 2, CustomAddFunction = "addMenuItem")]
    public List<CtrlMenuItem> ctrls;

    private CtrlMenuItem addMenuItem()
    {
        return new(client);
    }

    public InputField portInput;
    public Toggle connectButton;

    public Color connectBtnOnCol;
    public Color connectBtnOffCol;

    [DisplayAsString, LabelText("TCP Connected: "), GUIColor("@isConnectTcp? Color.green : Color.red")]
    public bool isConnectTcp = false;

    private void UpdateConnectBtn(bool isOn)
    {
        if (isOn)
        {
            if (client.ConnectToServer())
            {
                connectButton.targetGraphic.color = connectBtnOnCol;
            }
            else
            {
                connectButton.SetIsOnWithoutNotify(false);
                connectButton.targetGraphic.color = connectBtnOffCol;
            }
        }
        else
        {
            connectButton.targetGraphic.color = connectBtnOffCol;
            client.Close();
        }
        isConnectTcp = connectButton.isOn;
    }

    private void Start()
    {
        portInput.text = client.serverPort + "";
        connectButton.isOn = false;
        connectButton.onValueChanged.AddListener(UpdateConnectBtn);

        ctrls.ForEach(i => i.StartInit());
    }

    // Update is called once per frame
    void Update()
    {
        ctrls.ForEach(i => i.UpdateValue());

        int.TryParse(portInput.text, out client.serverPort);

        if(isConnectTcp && client.valueChanged)
        {
            client.SendMsg();
            client.valueChanged = false;
        }
    }
}
