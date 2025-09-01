using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
public class ListIndexEvents : MonoBehaviour
{
    private HorizontalOrVerticalLayoutGroup layoutGroup => GetComponent<HorizontalOrVerticalLayoutGroup>();
    [ShowInInspector, DisplayAsString]
    private RectTransform[] children => layoutGroup.Children();

    public UnityEvent<int> indexedEvent = new();
    public void TriggerByGameObject(RectTransform listItem)
    {
        int tirggeredIndex = children.ToList().IndexOf(listItem);
        indexedEvent.Invoke(tirggeredIndex);
        if (listSelection.indexSource.getIndex() == tirggeredIndex)
        {
            selectedEvent.Invoke(tirggeredIndex);
        }
    }

    public void TriggerIndexedEvent(int index)
    {
        indexedEvent.Invoke(index);
    }

    public void TriggerSelectedEvent(int index)
    {
        selectedEvent.Invoke(index);
    }

    public ListSelection listSelection;
    [ShowIf(nameof(listSelection))]
    public UnityEvent<int> selectedEvent = new();

    public void ShowInt(int input)
    {
        Debug.Log(input);
    }
}
