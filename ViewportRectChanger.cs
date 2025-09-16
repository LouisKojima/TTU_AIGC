using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System;

public class ViewportRectChanger : MonoBehaviour
{
    public ViewportRectDrawer rectDrawer;
    public List<Camera> targetCameras;

    public bool isDrawing = false;

    [ShowInInspector]
    public int currentSetting = 0;

    [Serializable]
    private class CameraLayout
    {
        public List<Rect> rects = new List<Rect>();
        public int version = 1;
    }

    [ShowInInspector]
    public string layoutPath => Path.Combine(Application.persistentDataPath, "viewport_layout.json");

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

        TryLoadAndApplyLayout();
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
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.S))
        {
            SaveLayout();
        }
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.L))
        {
            ForceLoadLayout();
        }
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.R))
        {
            ResetLayout(deleteSavedFile: true);
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

    private void SaveLayout()
    {
        try
        {
            var data = new CameraLayout();
            foreach (var cam in targetCameras)
            {
                if (cam == null)
                {
                    data.rects.Add(new Rect(0, 0, 0, 0));
                    continue;
                }
                data.rects.Add(cam.rect);
            }
            var json = JsonUtility.ToJson(data, true);
            var dir = Path.GetDirectoryName(layoutPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(layoutPath, json);
            Debug.Log($"[ViewportRectChanger] Saved layout to: {layoutPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ViewportRectChanger] SaveLayout failed: {e.Message}");
        }
    }

    private bool TryLoadAndApplyLayout()
    {
        try
        {
            if (!File.Exists(layoutPath)) return false;
            var json = File.ReadAllText(layoutPath);
            var data = JsonUtility.FromJson<CameraLayout>(json);
            if (data == null || data.rects == null || data.rects.Count == 0) return false;

            int n = Mathf.Min(targetCameras.Count, data.rects.Count);
            for (int i = 0; i < n; i++)
            {
                var cam = targetCameras[i];
                if (cam == null) continue;
                cam.gameObject.SetActive(true);
                cam.rect = data.rects[i];
                cam.targetDisplay = 0;
            }
            Debug.Log($"[ViewportRectChanger] Loaded layout from: {layoutPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ViewportRectChanger] LoadLayout failed: {e.Message}");
            return false;
        }
    }

    private void ForceLoadLayout()
    {
        bool ok = TryLoadAndApplyLayout();
        if (!ok)
        {
            Debug.LogWarning($"[ViewportRectChanger] No saved layout to load at: {layoutPath}");
        }
    }

    private void ResetLayout(bool deleteSavedFile)
    {
        try
        {
            rectDrawer.enabled = false;
            isDrawing = false;
            currentSetting = 0;

            targetCameras.ForEach(x =>
            {
                if (NotValid(x)) return;
                x.gameObject.SetActive(false);
                SetCameraRect(Rect.zero, x);
                x.targetDisplay = 0;
            });

            if (deleteSavedFile && File.Exists(layoutPath))
            {
                File.Delete(layoutPath);
                Debug.Log($"[ViewportRectChanger] Deleted saved layout: {layoutPath}");
            }
            else
            {
                Debug.Log("[ViewportRectChanger] Layout reset to zero rects (not deleted file).");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ViewportRectChanger] ResetLayout failed: {e.Message}");
        }
    }
}
