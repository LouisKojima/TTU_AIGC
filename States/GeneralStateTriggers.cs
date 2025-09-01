using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralStateTriggers : StateMachineBehaviour
{
    public delegate void A(params string[] input);
    [ShowInInspector]
    public A someAction;
    public UnityEvent onStateEnter = new ();
    public UnityEvent onStateExit = new ();
    public UnityEvent onStateUpdate = new ();

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        someAction();
        onStateEnter.Invoke();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateUpdate.Invoke();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateExit.Invoke();
    }
}

public class StateEvent : UnityEvent, IExposedPropertyTable
{
    public void ClearReferenceValue(PropertyName id)
    {
        throw new NotImplementedException();
    }

    public UnityEngine.Object GetReferenceValue(PropertyName id, out bool idValid)
    {
        throw new NotImplementedException();
    }

    public void SetReferenceValue(PropertyName id, UnityEngine.Object value)
    {
        throw new NotImplementedException();
    }
}