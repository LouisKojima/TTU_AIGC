using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
public class InfiniteList : MonoBehaviour
{
    public ScrollRect parentScrollView;

    private HorizontalOrVerticalLayoutGroup layoutGroup => GetComponent<HorizontalOrVerticalLayoutGroup>();
    private RectTransform selfTransform => (RectTransform)transform;
    private RectTransform[] children => layoutGroup.Children();
    private RectTransform[] initChildren;
    private int initChildrenCount;

    [BoxGroup]
    public bool manualInitWidth = false;
    [BoxGroup]
    [ShowIf(nameof(manualInitWidth))]
    public float initWidth = 0;

    // Start is called before the first frame update
    void Start()
    {
        initChildren = layoutGroup.Children();
        initChildrenCount = initChildren.Length;
        if(!manualInitWidth)
        {
            initWidth = selfTransform.rect.width;
        }
        //Debug.Log("init width: " + initWidth);
        //float initWidth = layoutGroup.ChildrenXs().Max() + layoutGroup.ChildrenWidths()[initChildrenCount - 1];
        //Debug.Log("init width: " + layoutGroup.ChildrenXs().Max() + " + " + layoutGroup.ChildrenWidths()[initChildrenCount - 1] + " = " + initWidth);
        foreach (RectTransform tr in initChildren)
        {
            Instantiate(tr, transform);
        }
        foreach (RectTransform tr in initChildren)
        {
            Instantiate(tr, transform);
        }
        selfTransform.anchoredPosition -= new Vector2(initWidth, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) return;
        //Debug.Log(parentScrollView.velocity);
        if (parentScrollView.velocity.x < 0 && -selfTransform.anchoredPosition.x > selfTransform.rect.width * 2 / 3)
        {
            //parentScrollView.horizontalNormalizedPosition += 0.5f;
            selfTransform.anchoredPosition += new Vector2(selfTransform.rect.width / 3, 0);
        }
        else if (parentScrollView.velocity.x > 0 && selfTransform.anchoredPosition.x + selfTransform.rect.width > selfTransform.rect.width * 2 / 3)
        {
            //parentScrollView.horizontalNormalizedPosition += 0.5f;
            selfTransform.anchoredPosition -= new Vector2(selfTransform.rect.width / 3, 0);
        }
    }

    public void Check()
    {
        if (-selfTransform.anchoredPosition.x > selfTransform.rect.width * 2 / 3)
        {
            //parentScrollView.horizontalNormalizedPosition += 0.5f;
            selfTransform.anchoredPosition += new Vector2(selfTransform.rect.width / 3, 0);
        }
        else if (selfTransform.anchoredPosition.x + selfTransform.rect.width > selfTransform.rect.width * 2 / 3)
        {
            //parentScrollView.horizontalNormalizedPosition += 0.5f;
            selfTransform.anchoredPosition -= new Vector2(selfTransform.rect.width / 3, 0);
        }
    }
}
