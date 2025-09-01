using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacialExpressionChange : StateMachineBehaviour
{
    public Texture2D[] textures;
    private float lastTextureSwapTime;
    public float speed =0.2f;
    private Texture originalSmileFace;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        lastTextureSwapTime = Time.time;
        originalSmileFace = animator.gameObject.GetComponentInChildren<Renderer>().materials[1].mainTexture;

    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Time.time - lastTextureSwapTime > speed)
        {
            lastTextureSwapTime = Time.time;
            int textureIndex = Random.Range(0, textures.Length);
            animator.gameObject.GetComponentInChildren<Renderer>().materials[1].mainTexture = textures[textureIndex];
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        lastTextureSwapTime = Time.time;
        animator.gameObject.GetComponentInChildren<Renderer>().materials[1].mainTexture = originalSmileFace;
    }
}
