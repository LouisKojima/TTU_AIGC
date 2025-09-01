using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class SlidePosition : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    ScrollRect rect;
    private float targetPosX, targetPosY;

    public float[] targetXs = new float[] { 0f, 0.5f, 1f };
    public bool doX = true;
    public float[] targetYs = new float[] { 0f, 0.5f, 1f };
    public bool doY = true;

    [Range(0, 2)]
    public float timeDelay = 0.5f;
    private float endDragTime = 0f;

    private bool isDrag = false;
    private bool isMouseDown = false;
    private bool isAnimating = false;
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        isAnimating = false;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        endDragTime = Time.time;
        calTargetPos();
    }

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<ScrollRect>();
        calTargetPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAnimating) 
            calTargetPos();
    }

    void LateUpdate()
    {
        isMouseDown = Input.GetMouseButton(0);
        if (!isDrag && (Time.time - endDragTime) > timeDelay && !isMouseDown)
        {
            UpdatePrevPos();
            if (doX)
                rect.ScrollToNormalizedX(targetPosX);
            if (doY)
                rect.ScrollToNormalizedY(targetPosY);
        }

        if (!isDrag && previousPosition == rect.normalizedPosition)
        {
            if (!stopped)
            {
                //onStop.Invoke();
                stopped = true;
                calTargetPos();
                InvokeOnStop();
                //if (doX) onStopX.Invoke(targetPosX);
                //if (doY) onStopY.Invoke(targetPosY);
            }
        }
        else
        {
            stopped = false;
            //isAnimating = false;
        }

        //if (!isDrag)
        //    Debug.Log(rect.velocity.x);
    }

    private Vector2 previousPosition = Vector2.zero;
    public UnityEvent onStop = new();
    public UnityEvent<float> onStopX = new();
    public UnityEvent<float> onStopY = new();
    public UnityEvent<int> onStopIndexX = new();
    public UnityEvent<int> onStopIndexY = new();
    [ShowInInspector]
    private bool stopped = false;

    public void InvokeOnStop()
    {
        int indexX = targetXs.ToList().IndexOf(targetPosX);
        int indexY = targetYs.ToList().IndexOf(targetPosY);
        onStop.Invoke();
        if (doX)
        { 
            onStopX.Invoke(targetPosX);
            onStopIndexX.Invoke(indexX);
        }
        if (doY)
        {
            onStopY.Invoke(targetPosY);
            onStopIndexY.Invoke(indexY);
        }
    }

    private void UpdatePrevPos()
    {
        previousPosition = rect.normalizedPosition;
    }

    // Calculate target position
    public void calTargetPos()
    {
        Vector2 pos = rect.normalizedPosition;
        if (doX) targetPosX = targetXs.OrderBy(p => Mathf.Abs(pos.x - p)).ToArray()[0];
        if (doY) targetPosY = targetYs.OrderBy(p => Mathf.Abs(pos.y - p)).ToArray()[0];

        //Debug.Log("Caled target pos: " + targetPosX);
    }

    public void ScrollToIndexX(int targetIndex)
    {
        //calTargetPos();
        isAnimating = true;
        if (targetIndex >= targetXs.Length || targetIndex < 0) return;
        targetPosX = targetXs[targetIndex];
    }

    /// <summary>
    /// Scroll to the next n-th target X position. Give a negative value to scroll in the other direction.
    /// </summary>
    /// <param name="dist">The next n-th target position, Can be negative to scroll backwards.</param>
    public void ScrollNextX(int dist)
    {
        int currentIndex = targetXs.ToList().IndexOf(targetPosX);
        int targetIndex = currentIndex + dist;
        ScrollToIndexX(targetIndex);
    }

    public void ScrollToIndexY(int targetIndex)
    {
        //calTargetPos();
        isAnimating = true;
        if (targetIndex >= targetYs.Length || targetIndex < 0) return;
        targetPosY = targetYs[targetIndex];
    }

    /// <summary>
    /// Scroll to the next n-th target Y position. Give a negative value to scroll in the other direction.
    /// </summary>
    /// <param name="dist">The next n-th target position, Can be negative to scroll backwards.</param>
    public void ScrollNextY(int dist)
    {
        int currentIndex = targetYs.ToList().IndexOf(targetPosY);
        int targetIndex = currentIndex + dist;
        ScrollToIndexY(targetIndex);
    }

    public void ScrollToIndex(int targetIndex, bool doX)
    {
        if (doX)
        {
            ScrollToIndexX(targetIndex);
        }
        else
        {
            ScrollToIndexY(targetIndex);
        }
    }

    /// <summary>
    /// Scroll to the next n-th target position. Give a negative value to scroll in the other direction.
    /// </summary>
    /// <param name="dist">The next n-th target position, Can be negative to scroll backwards.</param>
    /// <param name="doX">TRUE to scroll horizontally, FALSE to scroll vetically.</param>
    public void ScrollNext(int dist, bool doX)
    {
        if (doX)
        {
            ScrollNextX(dist);
        }
        else
        {
            ScrollNextY(dist);
        }
    }
}
