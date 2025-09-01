using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewStateBehaviour : StateMachineBehaviour
{
    public GameObject thisView;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        thisView.SetActive(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        thisView.SetActive(false);
    }
}
