using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class LayoutGroupExtension
{
    public static RectTransform[] Children(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.gameObject.GetComponentsInChildren<RectTransform>()
            .Where(x => x.parent == self.transform)
            .ToArray();
    }

    public static float[] ChildrenWidths(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Select(x => x.rect.width).ToArray();
    }

    public static float[] ChildrenHeights(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Select(x => x.rect.height).ToArray();
    }

    public static float[] ChildrenXs(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Select(x => x.anchoredPosition.x).ToArray();
    }
    public static float[] ChildrenYs(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Select(x => x.anchoredPosition.y).ToArray();
    }
    public static Vector3[] ChildrenScales(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Select(x => x.localScale).ToArray();
    }

    public static int ChildrenCount(this HorizontalOrVerticalLayoutGroup self)
    {
        return self.Children().Length;
    }

    public static int VisibleCount(this HorizontalOrVerticalLayoutGroup self, RectMask2D parentMask, out int firstIndex, out int lastIndex, out float firstVisibleSize, out float lastVisibleSize)
    {
        firstIndex = -1;
        lastIndex = -1;
        firstVisibleSize = -1f;
        lastVisibleSize = -1f;

        //RectMask2D parentMask = self.gameObject.GetComponentInParent<RectMask2D>();
        if (!parentMask)
        {
            return -1;
        }
        RectTransform parent = parentMask.rectTransform;

        float selfPaddingFirst;
        float selfCoordinate;
        float parentPaddingFirst;
        float parentPaddingLast;
        float parentSize;
        float[] childrenSizes;
        float[] childrenCooridinates;

        if (self is HorizontalLayoutGroup)
        {
            selfPaddingFirst = self.padding.left;
            selfCoordinate = ((RectTransform)self.transform).anchoredPosition.x;
            parentPaddingFirst = parentMask.padding.x;
            parentPaddingLast = parentMask.padding.z;
            parentSize = parent.rect.width;
            childrenSizes = self.ChildrenWidths();
            childrenCooridinates = self.ChildrenXs();
        }
        else if (self is VerticalLayoutGroup)
        {
            selfPaddingFirst = self.padding.bottom;
            selfCoordinate = ((RectTransform)self.transform).anchoredPosition.y;
            parentPaddingFirst = parentMask.padding.y;
            parentPaddingLast = parentMask.padding.w;
            parentSize = parent.rect.height;
            childrenSizes = self.ChildrenHeights();
            childrenCooridinates = self.ChildrenYs();
        }
        else
        {
            return -1;
        }

        //Debug.Log("self pad: " + selfPaddingFirst + ", self cod: " + selfCoordinate + ", parent padF: " + parentPaddingFirst + ", parent padL: " + parentPaddingLast + ", parent size: " + parentSize);

        bool FirstCondition(float x) =>
            x + selfCoordinate + selfPaddingFirst > parentPaddingFirst;
        bool LastCondition(float x) =>
            x + selfCoordinate + selfPaddingFirst < parentSize - parentPaddingLast;

        float upper, lower;
        int visibleCount = 0;
        for (int i = 0; i < self.ChildrenCount(); i++)
        {
            upper = childrenCooridinates[i] + childrenSizes[i];
            lower = childrenCooridinates[i];
            if (FirstCondition(upper) && LastCondition(lower))
            {
                visibleCount++;
                firstIndex = firstIndex == -1 ? i : firstIndex;
            }
            //Debug.Log(i + ". child cod: " + childrenCooridinates[i] + ", child size: " + childrenSizes[i] + ", conditions: " + FirstCondition(upper) + " " + LastCondition(lower));
        }
        lastIndex = firstIndex + visibleCount - 1;

        if (firstIndex != -1)
        {
            firstVisibleSize = (childrenSizes[firstIndex] + selfCoordinate + childrenCooridinates[firstIndex]) - (parentPaddingFirst);
            firstVisibleSize = MathF.Min(firstVisibleSize, childrenSizes[firstIndex]);
        }
        if (lastIndex != -1)
        {
            lastVisibleSize = (parentSize - parentPaddingLast) - (childrenCooridinates[lastIndex] + selfCoordinate);
            lastVisibleSize = MathF.Min(lastVisibleSize, childrenSizes[lastIndex]);
        }

        return visibleCount;
    }
}
