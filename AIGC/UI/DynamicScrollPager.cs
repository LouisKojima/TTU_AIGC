using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class DynamicScrollPager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public ScrollRect scrollRect;              // Scroll View
    public RectTransform viewport;             // Viewport
    public RectTransform content;              // Content
    public GameObject pagePrefab;              // Page (带 GridLayoutGroup)
    public GameObject itemPrefab;              // Item (缩略图)
    public Transform paginationContainer;      // 分页指示器容器
    public Sprite dotActiveSprite;             // 高亮点
    public Sprite dotInactiveSprite;           // 非高亮点

    [Header("Settings")]
    public int itemsPerPage = 6;               // 每页显示几个
    public string relativeFolder = "SkyboxPreviews"; // StreamingAssets 下的子目录

    [Header("Skybox Data")]
    public List<Material> skyboxMaterials = new();   // 对应的天空盒材质（未来扩展）
    private List<Sprite> loadedSprites = new();      // 当前加载的预览图
    private int selectedIndex = -1;                  // 当前选中的 Item
    private int currentPage = 0;
    private int pageCount = 0;
    private List<Image> dots = new();

    [Header("Paging Settings")]
    [Range(0.1f, 1f)]
    public float snapDuration = 0.25f; // 吸附动画时长
    private bool isDragging = false;

// 与 loadedSprites 一一对应的文件完整路径或文件名（例如 ".../StreamingAssets/.../foo.png"）
    [SerializeField]
    private List<string> loadedFileNames = new List<string>();

    public SkyboxItemGroup itemGroup;

    void Start()
    {
        StartCoroutine(LoadImagesAndBuild());
    }

    /// <summary>
    /// 刷新 ScrollView（运行时可调用，支持未来新增的 AIGC 资源）
    /// </summary>
    public void Refresh()
    {
        StartCoroutine(LoadImagesAndBuild());
    }

    /// <summary>
    /// 扫描文件夹，加载 PNG 和 JPG，生成 Sprite
    /// </summary>
    IEnumerator LoadImagesAndBuild()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, relativeFolder);

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("预览图文件夹不存在: " + folderPath);
            yield break;
        }

        // 清空已有数据
        loadedSprites.Clear();
        loadedFileNames.Clear();

        var files = new List<string>();
        files.AddRange(Directory.GetFiles(folderPath, "*.png"));
        files.AddRange(Directory.GetFiles(folderPath, "*.jpg"));

        foreach (string file in files)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + file))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("加载失败: " + file);
                }
                else
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                    Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    loadedSprites.Add(sp);
                    loadedFileNames.Add(file);
                }
            }
        }

        BuildPages();
    }

void BuildPages()
{
    // 清理旧内容
    foreach (Transform child in content) Destroy(child.gameObject);
    foreach (Transform dot in paginationContainer) Destroy(dot.gameObject);
    dots.Clear();
    selectedIndex = -1;

    int total = loadedSprites.Count;
    pageCount = Mathf.CeilToInt((float)total / itemsPerPage);

    // Content 尺寸
    float vpW = viewport.rect.width;
    float vpH = viewport.rect.height;
    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pageCount * vpW);
    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vpH);

    var group = itemGroup;
    if (group == null)
    {
        Debug.LogError("[DynamicScrollPager] itemGroup 未设置。请在 Inspector 把 SkyboxItemGroup 拖到此字段。");
    }
    else
    {
        // 如需重建时清空历史注册，可放开
        // group.Clear();
    }

    // 构建页面
    for (int p = 0; p < pageCount; p++)
    {
        GameObject page = Instantiate(pagePrefab, content);
        var le = page.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.preferredWidth = vpW;
            le.preferredHeight = vpH;
        }

        // 填充 item
        for (int i = 0; i < itemsPerPage; i++)
        {
            int idx = p * itemsPerPage + i;
            if (idx >= total) break;

            GameObject item = Instantiate(itemPrefab, page.transform);
            item.name = $"Item_{idx}";

            // 缩略图
            var thumb = item.GetComponentInChildren<Image>();
            if (thumb) thumb.sprite = loadedSprites[idx];

            //关键：拿到 SkyboxItem 脚本
            var skyboxItem = item.GetComponent<SkyboxItem>();
            if (skyboxItem == null)
            {
                Debug.LogError($"[DynamicScrollPager] ItemPrefab 缺少 SkyboxItem 组件：{item.name}");
            }
            else
            {
                // 写入该项的 png 文件名
                string fileName = null;
                if (loadedFileNames != null && idx < loadedFileNames.Count && !string.IsNullOrEmpty(loadedFileNames[idx]))
                    fileName = Path.GetFileName(loadedFileNames[idx]);
                skyboxItem.fileNameInStreamingAssets = fileName;

                // 注册到分组（互斥选中）
                if (group != null) group.Register(skyboxItem);

                // 明确把点击绑定到分组选中（双保险，不依赖 Awake 里是否已绑）
                var btn = item.GetComponent<Button>();
                if (btn != null && group != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => group.Select(skyboxItem));
                }
            }
        }
    }

    // 分页指示器
    for (int i = 0; i < pageCount; i++)
    {
        GameObject dot = new GameObject("Dot_" + i, typeof(RectTransform), typeof(Image));
        dot.transform.SetParent(paginationContainer, false);
        var img = dot.GetComponent<Image>();
        img.sprite = dotInactiveSprite;
        img.rectTransform.sizeDelta = new Vector2(50, 15);
        dots.Add(img);
    }

    UpdateDots(0);

    // 重置滚动
    content.anchoredPosition = Vector2.zero;
    currentPage = 0;
}


    /// <summary>
    /// 点击缩略图：第一次点击选中，再次点击应用
    /// </summary>
    void OnItemClicked(int index)
    {
        if (selectedIndex == index)
        {
            ApplySkybox(index);
        }
        else
        {
            SetSelected(index);
        }
    }

    void SetSelected(int index)
    {
        selectedIndex = index;
        Debug.Log("选中天空盒 index: " + index);
        // TODO: 这里可以更新选中边框、Apply 按钮的显示
    }

    void ApplySkybox(int index)
    {
        Debug.Log("应用天空盒 index: " + index);
        if (index < skyboxMaterials.Count)
        {
            RenderSettings.skybox = skyboxMaterials[index];
            DynamicGI.UpdateEnvironment();
        }
    }

    void UpdateDots(int active)
    {
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].sprite = (i == active) ? dotActiveSprite : dotInactiveSprite;
        }
    }

    // ========================
    // 横向分页吸附功能
    // ========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        StopAllCoroutines();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        SnapToNearestPage();
    }

    void SnapToNearestPage()
    {
        if (pageCount <= 0) return;

        float vpW = viewport.rect.width;
        float posX = -content.anchoredPosition.x; // 正数代表向右滚的距离
        int targetPage = Mathf.RoundToInt(posX / vpW);
        targetPage = Mathf.Clamp(targetPage, 0, pageCount - 1);

        StopAllCoroutines();
        StartCoroutine(SnapRoutine(targetPage));
    }

    IEnumerator SnapRoutine(int pageIndex)
    {
        float vpW = viewport.rect.width;
        Vector2 start = content.anchoredPosition;
        Vector2 end = new Vector2(-pageIndex * vpW, start.y);

        float t = 0;
        while (t < snapDuration)
        {
            t += Time.unscaledDeltaTime;
            content.anchoredPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0, 1, t / snapDuration));
            yield return null;
        }
        content.anchoredPosition = end;
        currentPage = pageIndex;
        UpdateDots(pageIndex);
    }
}
