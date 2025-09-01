using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavListCtrl : MonoBehaviour
{
    public NavLocations navLocations;
    public NavLineDrawer lineDrawer;
    public GameObject listItem;

    public HorizontalOrVerticalLayoutGroup listContainer;
    public ToggleGroup toggleGroup;

    public List<GameObject> listItems = new();

    public int chosenIndex;

    private void Reset()
    {
        navLocations = FindObjectOfType<NavLocations>();
        lineDrawer = FindObjectOfType<NavLineDrawer>();
        listContainer = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
        toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    public void SetAllOff()
    {
        listItems.ForEach(x => x.GetComponent<Toggle>().isOn = false);
    }

    public void SetChosen(int i)
    {
        listItems[i].GetComponent<Toggle>().isOn = true;
        chosenIndex = i;
    }

    public void StopNavi()
    {
        chosenIndex = -1;
        listItems.ForEach((x) => x.GetComponent<Toggle>().isOn = false);
    }

    [Button]
    public void UpdateList()
    {
        listItems.ForEach(x => DestroyImmediate(x));

        var s = navLocations.locations.Select((l,n) =>
        {
            Debug.Log(n + ". " + l);
            GameObject added = Instantiate
            (
                listItem,
                parent: listContainer.transform
            );
            added.tag = "PlayListItem";
            added.SetActive(true);
            added.name = l.ToString();
            added.GetComponentInChildren<TMP_Text>().text = l.ToString();

            Toggle toggle = added.GetComponentInChildren<Toggle>();

            void OnValueChanged(bool isOn)
            {
                if (isOn)
                {
                    lineDrawer.targetPos = navLocations.locations[n].position;
                    lineDrawer.StartDrawing();
                }
                else
                {
                    lineDrawer.StopDrawing();
                }
                //Debug.Log("Clicked: " + isOn);
            }

            toggle.onValueChanged.AddListener(OnValueChanged);
            toggle.group = toggleGroup;

            return 0;
        });
        s.ToList().Clear();

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        listItems = listContainer.GetComponentsInChildren<Transform>(false)
            .Where(t => t.tag == "PlayListItem" && t.parent == listContainer.transform)
            .Select(t => t.gameObject)
            .ToList();

        //SetChosen(chosenIndex);
        Debug.Log("Nav List Updated.");
    }

    public void UpdateListItem()
    {
        listItems.ForEach(x => x.GetComponent<PlayListItem>().Apply());
    }

    private void Start()
    {
        UpdateList();
    }
}
