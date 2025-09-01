using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
public class ListItemSizer : IndexSource
{
    public float defaultSize = 1f;
    public float highlightSize = 1.1f;
    public float highlightIndex = 0.5f;
    public float highlightRelativeToParent = 0.5f;
    public RectMask2D parentMask;

    private HorizontalOrVerticalLayoutGroup layoutGroup => GetComponent<HorizontalOrVerticalLayoutGroup>();
    private RectTransform selfTransform => (RectTransform)transform;

    [ShowInInspector, DisplayAsString]
    private RectTransform[] children => layoutGroup.Children();
    private float[] childrenXs => layoutGroup.ChildrenXs();
    private float[] childrenWidths => layoutGroup.ChildrenWidths();

    [ShowInInspector, DisplayAsString]
    private int visibleCount => layoutGroup.VisibleCount(parentMask, out firstVisibleIndex, out lastVisibleIndex, out firstVisibleWidth, out lastVisibleWidth);

    [ShowInInspector, DisplayAsString]
    private int firstVisibleIndex, lastVisibleIndex;

    [ShowInInspector, DisplayAsString]
    private float firstVisibleWidth, lastVisibleWidth;

    [ShowInInspector, DisplayAsString]
    private int currentHightlightIndex;

    public override int getIndex()
    {
        return currentHightlightIndex;
    }

    /// <summary>
    /// The position is measured by index + itemwise proportion.
    /// E.g. Halfway of the second item is 1.5
    /// </summary>
    /// <param name="position">
    /// The position measured by index + itemwise proportion.
    /// </param>
    public void CalculateByPositionIndex(float position)
    {
        //Debug.Log("CalculateByPositionIndex position: " + position);
        currentHightlightIndex = Mathf.FloorToInt(position);
        currentHightlightIndex = Mathf.Clamp(currentHightlightIndex, 0, children.Length - 1);
        float localPosition = position - currentHightlightIndex;
        float resultRatio = Mathf.PingPong(Mathf.Clamp(localPosition, 0, 1), 0.5f) * 2;
        ApplyHighlightRatio(currentHightlightIndex, resultRatio);
    }

    public void ApplyHighlightRatio(int index, float ratio)
    {
        children[index].localScale = Vector3.one * Mathf.LerpUnclamped(defaultSize, highlightSize, ratio);
        for (int i = 0; i < children.Length; i++)
        {
            if (i != index)
            {
                children[i].localScale = Vector3.one * defaultSize;
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void CalculateByVisible()
    {
        if (!parentMask) return;
        float visibleCoordinate = parentMask.padding.x - ((RectTransform)transform).anchoredPosition.x;
        float visibleProportion = (visibleCoordinate - childrenXs[firstVisibleIndex]) / childrenWidths[firstVisibleIndex];
        float position = highlightIndex + firstVisibleIndex + visibleProportion;
        currentHightlightIndex = Mathf.FloorToInt(position);

        CalculateByPositionIndex(position);
    }

    private float[] childrenInitWidths => children.Select(x => x.rect.width / x.localScale.x).ToArray();
    public void CalculateByParent()
    {
        float highlightPosition = highlightRelativeToParent * parentMask.rectTransform.rect.width - selfTransform.anchoredPosition.x;
        //Debug.Log("parent width: " + parentMask.rectTransform.rect.width + "; self x: " + selfTransform.anchoredPosition.x + "; highlightPosition: " + highlightPosition);

        int index = -1;
        for (int i = 0; i < children.Length; i++)
        {
            //Debug.Log("i = " + i + "; x: " + children[i].anchoredPosition.x + "; w:" + childrenInitWidths[i] + "; HL: " + highlightPosition + "; " + (children[i].anchoredPosition.x + childrenInitWidths[i] > highlightPosition) + children[i].rect.Contains(new Vector2(highlightPosition, 1)));
            if (children[i].anchoredPosition.x + childrenInitWidths[i] > highlightPosition && children[i].anchoredPosition.x < highlightPosition)
            //if (children[i].rect.Contains(new Vector2(highlightPosition,1)))
            //if(children[i].anchoredPosition.x <= highlightPosition && children[i].anchoredPosition.x + children[i].rect.width >= highlightPosition)
            {
                index = i;
                break;
            }
        }

        currentHightlightIndex = index;
        if (index == -1) return;

        float highlightProportion = (highlightPosition - childrenXs[currentHightlightIndex]) / childrenWidths[currentHightlightIndex];
        float position = currentHightlightIndex + highlightProportion;

        CalculateByPositionIndex(position);
    }

    /// <summary>
    /// The position is meatured by an overall proportion.
    /// E.g. If there are 5 items, halfway of the second item is 0.3
    /// </summary>
    /// <param name="position">
    /// The position meatured by an overall proportion from 0 to 1.
    /// </param>
    [Button]
    public void CalculateByPosition01
    (
        [PropertyRange(0, 1)]
        [OnValueChanged("@"+nameof(CalculateByPosition01)+"($value)")]
        float position
    )
    {
        float position01 = Mathf.Clamp01(position);
        float coodinate = position01 * children.Length;
        CalculateByPositionIndex(coodinate);
    }

    /// <summary>
    /// Similiar to <see cref="CalculateByPosition01"/>. Only considers the x value of Vector2.
    /// </summary>
    /// <param name="position">Only x value is used.</param>
    public void CalculateByVector2DHorizontal(Vector2 position)
    {
        CalculateByPosition01(position.x);
    }

    /// <summary>
    /// Similiar to <see cref="CalculateByPosition01"/>. Only considers the y value of Vector2.
    /// </summary>
    /// <param name="position">Only y value is used.</param>
    public void CalculateByVector2DVertical(Vector2 position)
    {
        CalculateByPosition01(position.y);
    }
}
