using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapScaleCtrl : MonoBehaviour
{
    private Vector2 changed;
    private Vector2 changedDist;
    private Vector2 initScale;
    [ShowInInspector, DisplayAsString]
    private int zoom = 0;

    [Range(1, 2)]
    public float step = 1.5f;
    public int minZoom = -1;
    public int maxZoom = 3;
    public ScrollRect mapScrollRect;

    [Button]
    public void SetMapPivot()
    {
        changed = mapScrollRect.content.pivot - mapScrollRect.normalizedPosition;
        changedDist = changed * mapScrollRect.content.rect.size * mapScrollRect.content.localScale;
        mapScrollRect.content.pivot = mapScrollRect.normalizedPosition;
        mapScrollRect.content.localPosition -= new Vector3(changedDist.x, changedDist.y);
    }

    [Button]
    public void SetScale(int input)
    {
        SetMapPivot();
        zoom = Mathf.Clamp(input, minZoom, maxZoom);
        mapScrollRect.content.localScale = initScale * Mathf.Pow(step, zoom);
    }

    public int GetScale()
    {
        return zoom;
    }

    [Button]
    public void ScaleUp()
    {
        SetScale(zoom + 1);
    }

    [Button]
    public void ScaleDown()
    {
        SetScale(zoom - 1);
    }

    private void Awake()
    {
        initScale = mapScrollRect.content.localScale;
    }
}
