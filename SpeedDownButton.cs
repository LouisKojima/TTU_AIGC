using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SpeedDownButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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
        if (isPress) { carControl.data.speed--; }
    }


    public void OnPointerDown(PointerEventData eventData)
    {

        isPress = true;


    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
    }
}
