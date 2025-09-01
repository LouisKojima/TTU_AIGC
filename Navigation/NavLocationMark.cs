using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavLocationMark : MonoBehaviour
{
    [ShowInInspector]
    public string title { get => gameObject.name; set => gameObject.name = value; }
    public Color gizmoColor = Color.green;
    [MinValue(0)]
    public float gizmoSize = 10;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}
