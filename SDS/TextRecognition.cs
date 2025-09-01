using IAVTypes.AmbientColor;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using static ColorChangerSlider;

public class TextRecognition : MonoBehaviour
{
    public ColorChangerSlider colorCtrl;
    //static void A() { }
    //static void A(int a) { }
    public Dictionary<string, Action> commands
    {
        get
        {
            Dictionary<string, Action> _temp = new()
            {
                //["a"] = () => A(1),
                //["b"] = A
                ["ambient"] = () => { ambientDetected = true; },
                ["light"] = () => { ambientDetected = true; },
                ["lights"] = () => { ambientDetected = true; },
                ["red"] = () => { if (ambientDetected) ChangeColor(AmbientColor.RED); },
                ["yellow"] = () => { if (ambientDetected) ChangeColor(AmbientColor.YELLOW); },
                ["green"] = () => { if (ambientDetected) ChangeColor(AmbientColor.GREEN); },
                ["blue"] = () => { if (ambientDetected) ChangeColor(AmbientColor.BLUE); },
                ["purple"] = () => { if (ambientDetected) ChangeColor(AmbientColor.PURPLE); },
                ["white"] = () => { if (ambientDetected) ChangeColor(AmbientColor.WHITE); },
            };
            Dictionary<string, Action> result = new();
            foreach ((string key, Action action) in _temp)
            {
                result.Add(key, action);
                result[key] += () => Debug.Log(key + " detected");
            }
            return result;
        }
    }

    public bool ambientDetected = false;
    [DisplayAsString]
    public string reply = "";

    public void ChangeColor(AmbientColor amb, string name = null)
    {
        reply = "";
        ColorSliderPosition csp = amb.GetValue();
        colorCtrl.SetColor(csp.color, csp.saturation, csp.brightness);
        if (name != null) reply = "Ambient set to " + name;
        else reply = "Ambient set to " + amb.GetName();
        Debug.Log(reply);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [Button]
    public void Recognize(string input = "ambient red")
    {
        ambientDetected = false;
        var result = commands
            //.Where(a => input.Contains(a.Key, StringComparison.OrdinalIgnoreCase))
            .Where(a => Regex.IsMatch(input, @"\b" + a.Key + @"\b", RegexOptions.IgnoreCase))
            .ToDictionary(a => a.Key, a => a.Value);
        string recognized = "";
        if (result.Count != 0)
        {
            foreach ((string key, Action action) in result)
            {
                recognized += key + " ";
                action();
            }
        }
        else
        {
            recognized = "No result";
            reply = "Sorry I cannot understand.";
        }
        Debug.Log("Total recognized: " + recognized);
    }
}