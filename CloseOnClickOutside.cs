using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Trigger events when clicked outside. Used for Popups, Drop-downs, etc.
/// Handles drag-outside-and-release cases correctly.
/// </summary>
public class CloseOnClickOutside : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool closeThisDown = true;
    public bool closeThisUp = true;
    //Events other than closing
    public UnityEvent onPointerDownOutside = new UnityEvent();
    public UnityEvent onPointerUpOutside = new UnityEvent();

    [ShowInInspector]
    public bool inside { get; private set; }

    // --- NEW ---
    // This flag tracks if the mouse button was pressed down *inside* this UI element.
    private bool _pointerDownInside = false;

    // Update is called once per frame
    void Update()
    {
        // --- LOGIC REWRITTEN ---

        // Check for mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            if (inside)
            {
                // Mouse down started inside the panel.
                _pointerDownInside = true;
            }
            else
            {
                // Mouse down happened outside the panel.
                if (gameObject.activeSelf)
                {
                    onPointerDownOutside.Invoke();
                    if (closeThisDown)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        // Check for mouse button up
        if (Input.GetMouseButtonUp(0))
        {
            // Only trigger up event if the mouse is currently outside
            // AND the click did NOT start inside the panel.
            if (gameObject.activeSelf && !inside && !_pointerDownInside)
            {
                onPointerUpOutside.Invoke();
                if (closeThisUp)
                {
                    gameObject.SetActive(false);
                }
            }
            
            // Reset the flag for the next click cycle.
            _pointerDownInside = false;
        }
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
        // Reset state when the object is disabled.
        inside = false;
        _pointerDownInside = false;
    }
}
