using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Controller for indicators on FPK. 
/// Indicators are used by their codes (indices in the list)
/// </summary>
public class IndicatorsCtrl : MonoBehaviour
{
    public GameObject indicatorsParent;
    //List of all indicator objs
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name"), ShowInInspector, SerializeField]
    public List<GameObject> indicatorsList = new();
    /// <summary>
    /// Add all children objs under indicatorsParent
    /// </summary>
    [Button]
    private void AddAll()
    {
        if (indicatorsParent == null) indicatorsParent = this.gameObject;
        indicatorsList = indicatorsParent
            .GetComponentsInChildren<RectTransform>(includeInactive: true)
            .Where(x => x.parent == this.transform)
            .Select(x => x.gameObject).ToList();
        indicatorsList.Sort((a, b) => a.name.CompareTo(b.name));
    }

    /// <summary>
    /// Sets the status of an indicator according to its code (index in the list)
    /// </summary>
    public void SetIndicator(int code, bool toSet)
    {
        indicatorsList.ElementAt(code)?.SetActive(toSet);
    }
}
