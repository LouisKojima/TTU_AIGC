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
    /// 扫描文件夹，加载 PNG，生成 Sprite
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

        string[] files = Directory.GetFiles(folderPath, "*.png");
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
                }
            }
        }

        BuildPages();
    }

    /// <summary>
    /// 构建 Scroll View 页面
    /// </summary>
    void BuildPages()
    {
        // 清理旧内容
        foreach (Transform child in content) Destroy(child.gameObject);
        foreach (Transform dot in paginationContainer) Destroy(dot.gameObject);
        dots.Clear();
        selectedIndex = -1;

        int total = loadedSprites.Count;
        pageCount = Mathf.CeilToInt((float)total / itemsPerPage);

        // 设置 Content 尺寸
        float vpW = viewport.rect.width;
        float vpH = viewport.rect.height;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pageCount * vpW);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vpH);

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
                var thumb = item.GetComponentInChildren<Image>();
                if (thumb) thumb.sprite = loadedSprites[idx];

                int captured = idx;
                var btn = item.GetComponent<Button>();
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnItemClicked(captured));
                }
            }
        }

        // 生成分页指示器
        for (int i = 0; i < pageCount; i++)
        {
            GameObject dot = new GameObject("Dot_" + i, typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(paginationContainer, false);
            var img = dot.GetComponent<Image>();
            img.sprite = dotInactiveSprite;
            img.rectTransform.sizeDelta = new Vector2(16, 6);
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
