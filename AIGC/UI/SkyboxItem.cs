// SkyboxItem.cs
using UnityEngine;
using UnityEngine.UI;

public class SkyboxItem : MonoBehaviour
{
    [Header("Refs")]
    public Button rootButton;          // 指向 ItemPrefab 根 Button（可留空，自动找）
    public Image thumbnail;            // 缩略图（可选）
    public GameObject selectionFrame;  // 渐变边框容器（默认隐藏）
    public Button applyButton;         // 右下角 Apply（默认隐藏）

    [Header("Data")]
    public string fileNameInStreamingAssets; // 例如 "my_sky_01.png"

    // 由管理器注入
    private SkyboxItemGroup _group;

    void Awake()
    {
        if (!rootButton) rootButton = GetComponent<Button>();
        if (rootButton)
        {
            rootButton.onClick.RemoveAllListeners();
            rootButton.onClick.AddListener(() => _group?.Select(this));
        }

        if (applyButton)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(OnApplyClicked);
        }

        SetSelected(false); // 初始不选
    }

    public void BindGroup(SkyboxItemGroup group) => _group = group;

    public void SetSelected(bool on)
    {
        if (selectionFrame) selectionFrame.SetActive(on);
        if (applyButton)    applyButton.gameObject.SetActive(on);
    }

    void OnApplyClicked()
    {
        _group?.Apply(this);
        Debug.Log($"[SkyboxItem.ApplyClicked] {name} file={fileNameInStreamingAssets} group={_group}");
    }
}
