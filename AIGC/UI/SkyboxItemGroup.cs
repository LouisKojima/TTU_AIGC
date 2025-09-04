// SkyboxItemGroup.cs
using System.Collections.Generic;
using UnityEngine;

public class SkyboxItemGroup : MonoBehaviour
{
    [Header("Refs")]
    public SkyboxController skyboxController; // 你的控制器（Inspector 拖上去）

    private readonly List<SkyboxItem> _items = new();
    private SkyboxItem _selected;

    // 供外部（比如动态生成时）注册
    public void Register(SkyboxItem item)
    {
        if (item == null || _items.Contains(item)) return;
        _items.Add(item);
        item.BindGroup(this);
        item.SetSelected(false);
    }

    // 可在重建列表前清空
    public void Clear()
    {
        _selected = null;
        _items.Clear();
    }

    public void Select(SkyboxItem item)
    {
        if (_selected == item) return;

        if (_selected) _selected.SetSelected(false);
        _selected = item;
        if (_selected) _selected.SetSelected(true);
    }

    public void Apply(SkyboxItem item)
    {
        if (item == null || skyboxController == null) return;
        if (!string.IsNullOrEmpty(item.fileNameInStreamingAssets))
            skyboxController.ApplyFromStreamingAssets(item.fileNameInStreamingAssets);
    }
}
