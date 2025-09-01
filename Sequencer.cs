using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sequencer : MonoBehaviour
{
    public List<UnityEvent> events = new();

    public void ExcecuteAtIndex(int index)
    {
        if (index > events.Count || index < 0) return;
        events[index].Invoke();
    }

    private int prevIndex = -1;
    public void ExcecuteAtIntexOnce(int index)
    {
        if (index == prevIndex) return;
        ExcecuteAtIndex(index);
        prevIndex = index;
    }

    public void ResetOnce()
    {
        prevIndex = -1;
    }
}
