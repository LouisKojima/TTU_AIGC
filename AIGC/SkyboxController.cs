using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SkyboxController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Material skyboxMaterial;                   // 直接在 Inspector 指向当前项目的天空盒材质

    [Header("Settings")]
    public string relativeFolder = "SkyboxPreviews";  // StreamingAssets 下的子目录
    public string texturePropertyName = "_MainTex";   // Skybox/Panoramic 默认主贴图属性

    /// <summary>
    /// 从 StreamingAssets/SkyboxPreviews/ 读取 PNG 并应用到天空盒材质。
    /// 传入文件名（如 "my_skybox.png"）。
    /// </summary>
    public void ApplyFromStreamingAssets(string fileName)
    {
        Debug.Log($"[SkyboxController] ApplyFromStreamingAssets file={fileName}");

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("[SkyboxController] fileName 为空。");
            return;
        }
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativeFolder, fileName);
        StartCoroutine(LoadAndApply(fullPath));
    }

    /// <summary>
    /// 直接使用完整文件路径（包含后缀）。
    /// </summary>
    public void ApplyFromFullPath(string fullPath)
    {
        StartCoroutine(LoadAndApply(fullPath));
    }

    private IEnumerator LoadAndApply(string fullPath)
    {
        Debug.Log($"[SkyboxController] LoadAndApply fullPath={fullPath}");

        if (skyboxMaterial == null)
        {
            Debug.LogError("[SkyboxController] 请在 Inspector 指定 skyboxMaterial。");
            yield break;
        }
        if (!skyboxMaterial.HasProperty(texturePropertyName))
        {
            Debug.LogError($"[SkyboxController] 材质不含贴图属性 {texturePropertyName}。");
            yield break;
        }
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            Debug.LogWarning("[SkyboxController] 文件不存在: " + fullPath);
            yield break;
        }

        using (var req = UnityWebRequestTexture.GetTexture("file://" + fullPath))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[SkyboxController] 加载失败: " + fullPath);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            if (tex == null)
            {
                Debug.LogWarning("[SkyboxController] 纹理为空: " + fullPath);
                yield break;
            }

            // 可按需调整采样（保持简单即可）
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            skyboxMaterial.SetTexture(texturePropertyName, tex);

            // 确保当前环境使用该材质（若已设置可省略）
            if (RenderSettings.skybox != skyboxMaterial)
                RenderSettings.skybox = skyboxMaterial;

            // 立刻刷新环境反射（可选）
            DynamicGI.UpdateEnvironment();
        }
    }
}
