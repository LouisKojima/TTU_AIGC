using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ViewportRectDrawer : MonoBehaviour//, IPointerDownHandler, IPointerUpHandler
{
    Texture2D selectTexture;

    Vector3 boxOrigin;
    Vector3 boxEnd;

    bool drawing;
    public Rect rect;

    public UnityEvent<Rect> onFinishRect = new();

    private void OnEnable()
    {
        drawing = false;
        selectTexture = new Texture2D(1, 1);
        selectTexture.SetPixel(0, 0, UnityEngine.Color.white);
        selectTexture.Apply();
        Debug.Log("Screen W: " + Screen.width + " H: " + Screen.height);
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
        //boxOrigin = Input.mousePosition;
        //drawing = true;
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
        //drawing = false;
        //onFinishRect.Invoke(rect);
        //Debug.Log(rect.ToString());
    //}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            boxOrigin = Input.mousePosition;
            drawing = true;
        }

        if (drawing)
        {
            boxEnd = Input.mousePosition;
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            drawing = false;
            onFinishRect.Invoke(rect);
            //Debug.Log(rect.ToString());
        }
    }

    public void CancelDrawing()
    {
        drawing = false;
    }

    void OnGUI()
    {
        if (drawing)
        {
            Rect area = new Rect(boxOrigin.x, Screen.height - boxOrigin.y, boxEnd.x - boxOrigin.x, boxOrigin.y - boxEnd.y);

            Rect lineArea = area;
            lineArea.height = 1; //Top line
            GUI.DrawTexture(lineArea, selectTexture);
            lineArea.y = area.yMax - 1; //Bottom
            GUI.DrawTexture(lineArea, selectTexture);
            lineArea = area;
            lineArea.width = 1; //Left
            GUI.DrawTexture(lineArea, selectTexture);
            lineArea.x = area.xMax - 1;//Right
            GUI.DrawTexture(lineArea, selectTexture);

            rect = area;
            rect.y = Screen.height - rect.y;
            rect.height = -rect.height;
        }
    }
}
