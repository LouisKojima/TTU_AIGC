using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonSpeedButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public UnityEvent onPress;
    public UnityEvent onClick;
    public CarControl carControl;

    private bool isDown = false;
    private bool isPress = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isDown)
        {
            Debug.Log("button is down in update");
        }
        if (isPress) { carControl.data.speed++; }
    }


    public void OnPointerDown(PointerEventData eventData)
    {        
        
        isPress= true;

        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress= false;
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    Debug.Log("onclick in onclick");
    //    carControl.data.speed++;
    //    Debug.Log("加速成功(OnClick)");
    //}
}
