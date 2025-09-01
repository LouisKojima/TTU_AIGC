using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// Trigger events when clicked outside. Used for Popups, Drop-downs, etc.
/// </summary>
public class CloseOnClickOutside : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool closeThisDown = true;
    public bool closeThisUp = true;
    //Events other than closing
    public UnityEvent onPointerDownOutside = new UnityEvent();
    public UnityEvent onPointerUpOutside = new UnityEvent();

    [ShowInInspector]
    public bool inside { get; set; }

    private void HideIfClickedOutside(GameObject panel)
    {
        if (Input.GetMouseButtonDown(0) && panel.activeSelf && !inside)
        {
            //Trigger events
            onPointerDownOutside.Invoke();
            if (closeThisDown)
            {
                panel.SetActive(false);
            }
        }
        if (Input.GetMouseButtonUp(0) && panel.activeSelf && !inside)
        {
            //Trigger events
            onPointerUpOutside.Invoke();
            if (closeThisUp)
            {
                panel.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HideIfClickedOutside(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inside = false;
    }

    private void OnDisable()
    {
        inside = false;
    }
}
