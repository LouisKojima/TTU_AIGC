using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LifecycleEvents : MonoBehaviour
{
    public UnityEvent onStart;
    public UnityEvent onEnable;
    public UnityEvent onDisable;
    public UnityEvent onUpdate;

    void Start()
    {
        if (onStart.GetPersistentEventCount() == 0) return;
        onStart.Invoke();
    }

    void Update()
    {
        if (onUpdate.GetPersistentEventCount() == 0) return;
        onUpdate.Invoke();
    }

    private void OnDisable()
    {
        if (onDisable.GetPersistentEventCount() == 0) return;
        onDisable.Invoke();
    }

    private void OnEnable()
    {
        if (onEnable.GetPersistentEventCount() == 0) return;
        onEnable.Invoke();
    }


}
