using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCamCtrl : MonoBehaviour
{
    [Required]
    public List<Camera> navCams;
    [Required]
    public Transform pivit1;
    [Required]
    public Transform pivit2;
    [Required]
    public Transform target;
    public bool fixedUp = false;
    [Title("Zoom")]
    public float defaultSize = 128;
    //public float defaultField = 80;
    [MinValue(1)]
    public float zoomStep = 1.5f;
    public float zoomLevel = 0;
    [MinMaxSlider(-5, 5, showFields: true)]
    public Vector2Int zoomRange = new Vector2Int(-3, 4);
    private float zoomFactor => Mathf.Pow(zoomStep, zoomLevel);
    [Title("Offset")]
    public float offsetY = 120;
    [ShowInInspector]
    public Vector2 offset { get; set; }
    [ShowInInspector, SerializeField]
    private Vector2 offsetBias;

    [Button]
    public void ZoomOut()
    {
        zoomLevel = Mathf.Clamp(zoomLevel + 1, zoomRange.x, zoomRange.y);
        SetZoom();
    }

    [Button]
    public void ZoomIn()
    {
        zoomLevel = Mathf.Clamp(zoomLevel - 1, zoomRange.x, zoomRange.y);
        SetZoom();
    }

    public void SetZoom(float level)
    {
        zoomLevel = Mathf.Clamp(level, zoomRange.x, zoomRange.y);
        SetZoom();
    }

    public void SetZoom()
    {
        if (navCams[0].orthographic)
            navCams.ForEach(x => x.orthographicSize = defaultSize * zoomFactor);
    }

    public void SetFixedUp(bool toset)
    {
        fixedUp = toset;
    }

    private void OnValidate()
    {
        Update();
        SetZoom();
    }

    void Update()
    {
        //float finalOffsetY = navCams[0].orthographic ? offsetY : offsetY * zoomFactor;
        Vector3 pos = target.position + offsetY * Vector3.up;
        Quaternion rot = target.transform.rotation;
        rot.eulerAngles = new(
            pivit1.rotation.eulerAngles.x,
            fixedUp ? 0 : rot.eulerAngles.y,
            0);
        pivit1.SetPositionAndRotation(pos, rot);
        if (navCams[0].orthographic)
        {
            pivit2.localPosition = zoomFactor * new Vector3(offset.x+ offsetBias.x, 0, offsetBias.y+offset.y);
        }
        else
        {
            pivit2.localPosition = zoomFactor * new Vector3(offset.x, 0, offset.y);
            transform.localPosition = new Vector3(offsetBias.x, 0, offsetBias.y + offsetY * (1 - zoomFactor));
        }
    }
}
