using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Controlls panel switching used by menu tabs
/// </summary>
public class PanelSwitch : MonoBehaviour
{
    public List<GameObject> panels;
    // Initialize
    private void Reset()
    {
        // Find all children panels 
        panels = GetComponentsInChildren<Transform>(true)
            .Where(t => t.tag == "Panel" && t.parent == this.transform)
            .Select(t => t.gameObject)
            .ToList();
    }

    // Switch to a panel in the panels list
    public void SwitchTo(GameObject target)
    {
        // Check if target is valid
        if (!panels.Contains(target))
        {
            return;
        }
        // Activate the target panel, deactivate the others
        foreach (var panel in panels)
        {
            panel.gameObject.SetActive(panel.Equals(target));
        }
    }
}
