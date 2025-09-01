using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static VehicleStatusData;

namespace IAVTools
{
    public static class FieldGenerator
    {
        /// <summary>
        /// Generate UI's based on the parameters in target.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <returns>Generated UI's as a List</returns>
        public static List<(FieldInfo, GameObject)> GenerateField(this Transform transform, object target)
        {
            return transform.GenerateField(target, new string[] { });
        }

        /// <summary>
        /// Generate UI's based on the parameters in target with names for exclutions.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <param name="exludeFields"></param>
        /// <returns>Generated UI's as a List</returns>
        public static List<(FieldInfo, GameObject)> GenerateField(this Transform transform, object target, string[] exludeFields )
        {
            List<(FieldInfo, GameObject)> fieldObjs = new();

            Type type = target.GetType();
            //Fields are just variables
            FieldInfo[] fields = type.GetFields();
            //Exclude these
            //FieldInfo[] fieldX = exludeFields.Select(x => type.GetField(x)).ToArray();

            foreach (var field in fields)
            {
                if (exludeFields.Any(x => x.Equals(field.Name))) continue;
                //Public fields
                if (field.IsPublic)
                {
                    Debug.Log("field " + field.ToString() + " type: " + field.FieldType.ToString());
                    GameObject toAdd = AddChild(transform, new(field.Name, typeof(HorizontalLayoutGroup)));

                    //Add name on the left
                    GameObject nameObj = AddChild(toAdd, new("name", typeof(TextMeshProUGUI)));
                    nameObj.GetComponent<TextMeshProUGUI>().text = field.Name;

                    //Add input fields
                    if (field.FieldType.IsPrimitive)
                    {
                        switch (field.FieldType.Name)
                        {
                            //Boolean => A button where Green = true, Red = false
                            case nameof(Boolean):
                                GameObject toggleObj = AddChild(toAdd, new("toggle", typeof(Image), typeof(Toggle)));
                                toggleObj.GetComponent<Toggle>().targetGraphic = toggleObj.GetComponent<Image>();
                                toggleObj.GetComponent<Toggle>().isOn = (bool)field.GetValue(target);
                                break;

                            //Else => Text input
                            default:
                                GameObject textInputObj = AddChild(toAdd, new("field", typeof(TextMeshProUGUI), typeof(TMP_InputField)));
                                textInputObj.GetComponent<TMP_InputField>().textComponent = textInputObj.GetComponent<TextMeshProUGUI>();
                                textInputObj.GetComponent<TMP_InputField>().text = field.GetValue(target) + "";
                                break;
                        }
                    }
                    else
                    {
                        //Non-primitives names can be a mess, so switch type instead
                        switch (field.FieldType)
                        {
                            //Transmission => Text input
                            case var t when t == typeof(Transmission):
                                GameObject textInputObj = AddChild(toAdd, new("field", typeof(TextMeshProUGUI), typeof(TMP_InputField)));
                                textInputObj.GetComponent<TMP_InputField>().textComponent = textInputObj.GetComponent<TextMeshProUGUI>();
                                textInputObj.GetComponent<TMP_InputField>().text = field.GetValue(target).ToString();
                                break;

                            //List of indicators => Each indicator as boolean
                            case var t when t == typeof(List<Indicator>):
                                toAdd.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
                                GameObject togglesObj = AddChild(toAdd, new("toggles", typeof(HorizontalLayoutGroup)));
                                togglesObj.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
                                List<Indicator> toggles = (List<Indicator>)field.GetValue(target);
                                toggles.ForEach(x =>
                                {
                                    GameObject toggleObj = AddChild(togglesObj, new(x.name, typeof(TextMeshProUGUI), typeof(Toggle)));
                                    toggleObj.transform.rotation = Quaternion.Euler(new(0, 0, -45));
                                    toggleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                                    toggleObj.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
                                    toggleObj.GetComponent<TextMeshProUGUI>().text = x.name.StartsWith("Icon") ? x.name.Substring(4) : x.name;
                                    toggleObj.GetComponent<Toggle>().targetGraphic = toggleObj.GetComponent<TextMeshProUGUI>();
                                    toggleObj.GetComponent<Toggle>().isOn = x.isOn;
                                });
                                break;

                            //Others => Not supported
                            default:
                                break;
                        }
                    }
                    fieldObjs.Add((field, toAdd));
                }
            }
            ////Properties are those with getters & setters
            //PropertyInfo[] ppts = type.GetProperties();
            ////Exclude these
            //PropertyInfo[] pptX =
            //{
            //    type.GetProperty("name"),
            //    type.GetProperty("hideFlags"),
            //};
            //foreach (var ppt in ppts.Where(x => pptX.All(_ => !_.Equals(x))))
            //{
            //    Debug.Log("ppt " + ppt.ToString());
            //    GameObject toAdd = new(ppt.Name, typeof(TextMeshProUGUI));
            //    toAdd.GetComponent<TextMeshProUGUI>().text = ppt.Name;
            //    toAdd.transform.SetParent(transform, false);
            //}
            return fieldObjs;
        }

        /// <summary>
        /// Update parameters in target based on generated UI's.  
        /// </summary>
        /// <param name="fieldObjs"></param>
        /// <param name="target"></param>
        public static void UpdateFields(this List <(FieldInfo, GameObject)> fieldObjs, object target )
        {
            //Update accordingly
            //Wrong text inputs will not work
            fieldObjs.ForEach(x =>
            {
                FieldInfo field = x.Item1;
                GameObject obj = x.Item2;
                if (field.FieldType.IsPrimitive)
                {
                    switch (field.FieldType.Name)
                    {
                        case nameof(Boolean):
                            bool isOn = obj.GetComponentInChildren<Toggle>().isOn;
                            field.SetValue(target, isOn);
                            obj.GetComponentInChildren<Image>().color = isOn ? Color.green : Color.red;
                            break;
                        default:
                            setPrimitiveValue(target, field, obj.GetComponentInChildren<TMP_InputField>().text);
                            break;
                    }
                }
                else
                {
                    switch (field.FieldType)
                    {
                        case var t when t == typeof(Transmission):
                            setEnumValue<Transmission>(target, field, obj.GetComponentInChildren<TMP_InputField>().text);
                            break;
                        case var t when t == typeof(List<Indicator>):
                            List<Indicator> toggles = (List<Indicator>)field.GetValue(target);
                            toggles.ForEach(x =>
                            {
                                GameObject thisToggle = obj.transform.Find("toggles").Find(x.name).gameObject;
                                x.isOn = thisToggle.GetComponent<Toggle>().isOn;
                                thisToggle.GetComponent<TextMeshProUGUI>().color = x.isOn ? Color.green : Color.red;
                            });
                            field.SetValue(target, toggles);
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        //Add Child and return it
        public static GameObject AddChild(GameObject parent, GameObject toAdd)
        {
            return AddChild(parent.transform, toAdd);
        }

        public static GameObject AddChild(Transform parent, GameObject toAdd)
        {
            toAdd.transform.SetParent(parent, false);
            return toAdd;
        }

        //Parse value of a field from a text
        private static void setPrimitiveValue(object target, FieldInfo field, string text)
        {
            field.SetValue(target, Convert.ChangeType(text, field.FieldType));
        }

        private static void setEnumValue<TEnum>(object target, FieldInfo field, string text) where TEnum : struct
        {
            field.SetValue(target, Enum.Parse<TEnum>(text, ignoreCase: true));
        }
    }
}