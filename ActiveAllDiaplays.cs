using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activate display 2 in Builds
/// </summary>
public class ActiveAllDiaplays : MonoBehaviour
{
    void Awake()
    {
        bool exec = true;
#if UNITY_EDITOR
        exec = false;
#endif
        if (exec)
        {
            Debug.Log("displays connected: " + Display.displays.Length);
            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.
            //Screen.fullScreen = false;
            for (int i = 1; i < Display.displays.Length; i++)
            {
                //Screen.fullScreen = false;
                //Display.displays[i].SetParams(1920, 1080, 0, 0);
                //Display.displays[i].Activate(1920, 1080, 60);
                Debug.Log("Acitvate Display " + i);
            }
        }       
    }
}
