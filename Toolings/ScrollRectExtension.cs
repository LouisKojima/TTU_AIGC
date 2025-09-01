using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtension
{
    public static void ScrollToNormalizedX(this ScrollRect rect, float targetPosX)
    {
        rect.horizontalNormalizedPosition = Mathf.Lerp(
            rect.horizontalNormalizedPosition, 
            targetPosX, 
            Time.deltaTime * 10);
    }
    public static void ScrollToNormalizedY(this ScrollRect rect, float targetPosY)
    {
        rect.verticalNormalizedPosition = Mathf.Lerp(
            rect.verticalNormalizedPosition, 
            targetPosY, 
            Time.deltaTime * 10);
    }
    public static void ScrollToNormalized(this ScrollRect rect, float targetPosX, float targetPosY)
    {
        rect.ScrollToNormalizedX(targetPosX);
        rect.ScrollToNormalizedY(targetPosY);
    }
}
