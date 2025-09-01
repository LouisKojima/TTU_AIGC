using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroll3D : MonoBehaviour
{
    public Transform target3D;

    private Vector3 startRotation;
    private Vector2 startScroll;
    private Vector2 lastScroll;

    private void Start()
    {
        lastScroll = Vector2.zero;
        SetStart();
    }

    public void SetStart()
    {
        startRotation = target3D.rotation.eulerAngles;
        startScroll = lastScroll;
    }

    public void apply3DRotation(Vector2 scroll)
    {
        lastScroll = scroll;
        var rotation = Quaternion.Euler(new Vector3(
            scroll.y - startScroll.y,
            startScroll.x - scroll.x,
            0
        ) + startRotation);
        target3D.rotation = rotation;
    }
}
