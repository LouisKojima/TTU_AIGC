using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorEvents : MonoBehaviour
{
    public UnityEvent OnExit = new();

    public void InvokeOnExit()
    {
        OnExit.Invoke();
    }
}
