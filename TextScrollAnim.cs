using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollAnim : MonoBehaviour
{
    ScrollRect scrollRect => GetComponent<ScrollRect>();
    [Min(0.1f)]
    public float duration = 2f;
    [Min(0f)]
    public float startDelay = 1f;
    [Min(0f)]
    public float endDelay = 0.5f;

    public bool play { get => _play; set => _play = value; }
    [SerializeField]
    private bool _play = true;
    public bool doX = true;
    public bool doY = false;

    public Graphic etc;

    private float startTime = 0f;

    public bool isOverflow => scrollRect.content.rect.width > scrollRect.GetComponent<RectTransform>().rect.width;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (etc)
            etc.gameObject.SetActive(isOverflow && !play);

        if (isOverflow && play)
        {
            if (startTime == 0f)
                startTime = Time.realtimeSinceStartup;
            if (doX)
                scrollRect.horizontalNormalizedPosition = AnimPos();
            if (doY)
                scrollRect.verticalNormalizedPosition = AnimPos();
        }
        else
        {
            startTime = 0f;
        }
    }

    private float AnimPos()
    {
        float passedTime = Time.realtimeSinceStartup - startTime;
        float singleLoopTime = Mathf.Repeat(passedTime, startDelay + duration + endDelay);

        if (singleLoopTime <= startDelay)
            return 0;
        if (singleLoopTime >= startDelay + duration)
            return 1;

        return (singleLoopTime - startDelay) / duration;
    }

    private void OnDisable()
    {
        if (doX)
            scrollRect.horizontalNormalizedPosition = 0;
        if (doY)
            scrollRect.verticalNormalizedPosition = 0;
        if (etc)
        {
            etc.gameObject.SetActive(true);
            etc.enabled = isOverflow;
        }
    }

    private void OnEnable()
    {
        if (etc)
            etc.gameObject.SetActive(false);
    }

    public void ResetAnimPos()
    {
        if (doX)
            scrollRect.horizontalNormalizedPosition = 0;
        if (doY)
            scrollRect.verticalNormalizedPosition = 0;

        if (isOverflow && play)
        {
            startTime = Time.realtimeSinceStartup;
        }
        else
        {
            startTime = 0f;
        }
    }
}
