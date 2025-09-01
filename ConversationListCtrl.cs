using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationListCtrl : MonoBehaviour
{
    [AssetsOnly]
    public GameObject leftBubble;
    [AssetsOnly]
    public GameObject rightBubble;

    public VerticalLayoutGroup listContainer;

    public GameObject placeHolder;

    public List<GameObject> conversations = new();

    [Button]
    public void UpdateList()
    {
        conversations = listContainer.GetComponentsInChildren<Transform>(true)
            .Where(t => t.tag == "Conversation" && t.parent == listContainer.transform)
            .Select(t => t.gameObject)
            .ToList();

        if(placeHolder)
        {
            placeHolder.SetActive(conversations.Count == 0);
        }
    }

    public enum Side { Left, Right }

    private string lastAdded = "";

    [Button]
    public void AddConversation([MultiLineProperty] string input, Side side)
    //{
    //    if (input.Equals(lastAdded)) return;
    //    lastAdded = input;
    //    AddConversationRaw(input, side);
    //}

    //[Button]
    //public void AddConversationRaw([MultiLineProperty] string input, Side side)
    {
        GameObject added = Instantiate(
            side == Side.Left ? leftBubble : rightBubble,
            parent: listContainer.transform);
        added.GetComponentInChildren<TMP_Text>().text = input;

        UpdateList();

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        Vector3 pos = listContainer.transform.position;
        listContainer.transform.position.Set(pos.x, 0, pos.z);
    }

    [Button]
    public void RemoveConversation(int index = 0)
    {
        if (index < 0 || conversations.Count <= index) return;
        DestroyImmediate(conversations[index]);
        //conversations.RemoveAt(index);
        UpdateList();
    }

    [Button]
    public void RemoveLastConversation()
    {
        RemoveConversation(conversations.Count - 1);
        UpdateList();
    }

    [Button]
    public void RemoveAllConversations()
    {
        conversations.ForEach(x => DestroyImmediate(x));
        UpdateList();
    }

    [Button]
    public void SetText(int index, [MultiLineProperty] string input)
    {
        if (index < 0 || conversations.Count <= index) return;
        conversations[index].GetComponentInChildren<TMP_Text>().text = input;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    [Button]
    public void SetLastText([MultiLineProperty] string input)
    {
        SetText(conversations.Count - 1, input);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void LateUpdate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
