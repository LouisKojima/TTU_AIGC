using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExecuteInEditor : MonoBehaviour
{
    public UnityEvent onStartEditor = new();
    public UnityEvent onStartBuild = new();
    public UnityEvent onUpdateEditor = new();
    public UnityEvent onUpdateBuild = new();
    void Awake()
    {
        if(Application.isEditor)
        {
            onStartEditor.Invoke();
        }
        else
        {
            onStartBuild.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            onUpdateEditor.Invoke();
        }
        else
        {
            onUpdateBuild.Invoke();
        }
    }
}
