using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViewportRectChanger : MonoBehaviour
{
    public ViewportRectDrawer rectDrawer;
    public List<Camera> targetCameras;

    public bool isDrawing = false;

    [ShowInInspector]
    public int currentSetting = 0;

    private bool NotValid(Camera x)
    {
        return
            !x
            //|| !x.gameObject.activeInHierarchy
            || !x.enabled;
    }

    private Camera currentCam => targetCameras[currentSetting];

    private void NextCam()
    {
        if (targetCameras.TrueForAll(x => NotValid(x)))
        {
            return;
        }

        currentSetting = (currentSetting + 1) % targetCameras.Count;

        if (NotValid(currentCam))
        {
            NextCam();
            return;
        }

        previousActive = currentCam.gameObject.activeSelf;
    }

    public void SetCameraRect(Rect rect)
    {
        isDrawing = false;

        SetCameraRect(rect, currentCam);

        NextCam();
    }
    public void SetCameraRect(Rect rect, Camera cam)
    {
        //Debug.Log(rect.ToString() + ", yMin: " + rect.yMin + ", yMax:" + rect.yMax);
        float x = Mathf.Min(rect.xMax, rect.xMin);
        x /= Screen.width;
        float y = Mathf.Min(rect.yMax, rect.yMin);
        y /= Screen.height;
        float w = rect.width / Screen.width;
        w = Mathf.Abs(w);
        float h = rect.height / Screen.height;
        h = Mathf.Abs(h);

        cam.gameObject.SetActive(true);
        cam.rect = new(x, y, w, h);
    }

    private void OnEnable()
    {
        rectDrawer.enabled = false;
        isDrawing = false;
        targetCameras.ForEach(x => 
        {
            if (NotValid(x)) return;
            x.gameObject.SetActive(false);
            SetCameraRect(Rect.zero,x);
            x.targetDisplay = 0;
        });
    }

    private bool previousActive = false;

    public void StartDrawRect()
    {
        Debug.Log("Drawing Rect for Camera: " + currentCam.gameObject.name);
        rectDrawer.enabled = true;
        isDrawing = true;
        currentCam.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if(Input.GetKey(KeyCode.Q))
            {
                StartDrawRect();
            }
        }
        if (isDrawing && Input.GetKey(KeyCode.Mouse1))
        {
            isDrawing = false;
            rectDrawer.CancelDrawing();
            rectDrawer.enabled = false;
            currentCam.gameObject.SetActive(previousActive);
            NextCam();
        }
        if (Input.GetKey(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
