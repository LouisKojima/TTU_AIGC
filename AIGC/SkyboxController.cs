using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SkyboxController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Material skyboxMaterial;

    [Header("Settings")]
    public string relativeFolder = "SkyboxPreviews";
    public string texturePropertyName = "_MainTex";

    private Material runtimeMaterial;
    private Texture2D runtimeTexture;

    private const string LastTextureKey = "SkyboxController.LastTexturePath";

    private void OnEnable()
    {
        if (!EnsureRuntimeMaterial())
        {
            return;
        }

        TryRestoreLastSkybox();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (RenderSettings.skybox == runtimeMaterial && skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }

        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
            runtimeTexture = null;
        }

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }
    }

    public void ApplyFromStreamingAssets(string fileName)
    {
        Debug.Log($"[SkyboxController] ApplyFromStreamingAssets file={fileName}");

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("[SkyboxController] fileName is empty.");
            return;
        }

        string fullPath = Path.Combine(Application.streamingAssetsPath, relativeFolder, fileName);
        StartCoroutine(LoadAndApply(fullPath));
    }

    public void ApplyFromFullPath(string fullPath)
    {
        StartCoroutine(LoadAndApply(fullPath));
    }

    public void ApplyLatestFromStreamingAssets()
    {
        string folder = Path.Combine(Application.streamingAssetsPath, relativeFolder ?? string.Empty);
        if (!Directory.Exists(folder))
        {
            Debug.LogWarning("[SkyboxController] Folder not found: " + folder);
            return;
        }

        string latest = FindLatestImagePath(folder);
        if (string.IsNullOrEmpty(latest))
        {
            Debug.LogWarning("[SkyboxController] No image found (.png/.jpg/.jpeg).");
            return;
        }

        Debug.Log("[SkyboxController] Applying latest skybox: " + latest);
        StartCoroutine(LoadAndApply(latest));
    }

    private static string FindLatestImagePath(string folder)
    {
        try
        {
            var exts = new string[] { ".png", ".jpg", ".jpeg" };
            string latest = null;
            DateTime latestTime = DateTime.MinValue;

            foreach (var file in Directory.GetFiles(folder))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (Array.IndexOf(exts, ext) < 0) continue;
                var info = new FileInfo(file);
                var t = info.LastWriteTimeUtc;
                if (t > latestTime)
                {
                    latestTime = t;
                    latest = file;
                }
            }
            return latest;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SkyboxController] Failed to choose latest image: " + ex.Message);
            return null;
        }
    }

    private IEnumerator LoadAndApply(string fullPath, bool rememberPath = true)
    {
        Debug.Log($"[SkyboxController] LoadAndApply fullPath={fullPath}");

        if (!EnsureRuntimeMaterial())
        {
            yield break;
        }

        if (!TryNormalizeFullPath(fullPath, out string normalizedPath))
        {
            yield break;
        }

        if (!File.Exists(normalizedPath))
        {
            Debug.LogWarning("[SkyboxController] File does not exist: " + normalizedPath);
            yield break;
        }

        if (!runtimeMaterial.HasProperty(texturePropertyName))
        {
            Debug.LogError("[SkyboxController] Material misses texture property " + texturePropertyName);
            yield break;
        }

        using (var req = UnityWebRequestTexture.GetTexture("file://" + normalizedPath))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[SkyboxController] Load failed: " + normalizedPath);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            if (tex == null)
            {
                Debug.LogWarning("[SkyboxController] Texture is null: " + normalizedPath);
                yield break;
            }

            if (runtimeTexture != null && runtimeTexture != tex)
            {
                Destroy(runtimeTexture);
            }

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            runtimeMaterial.SetTexture(texturePropertyName, tex);
            runtimeTexture = tex;

            const string tintProp = "_Tint";
            const string exposureProp = "_Exposure";
            if (runtimeMaterial.HasProperty(tintProp))
            {
                runtimeMaterial.SetColor(tintProp, new Color32(0xCF, 0xCF, 0xCF, 0xFF));
            }
            if (runtimeMaterial.HasProperty(exposureProp))
            {
                runtimeMaterial.SetFloat(exposureProp, 0.7f);
            }

            if (RenderSettings.skybox != runtimeMaterial)
            {
                RenderSettings.skybox = runtimeMaterial;
            }

            DynamicGI.UpdateEnvironment();

            if (rememberPath)
            {
                RememberLastUsedPath(normalizedPath);
            }
        }
    }

    private bool EnsureRuntimeMaterial()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("[SkyboxController] Assign skyboxMaterial in Inspector.");
            return false;
        }

        if (runtimeMaterial == null)
        {
            runtimeMaterial = new Material(skyboxMaterial);
        }

        if (RenderSettings.skybox != runtimeMaterial)
        {
            RenderSettings.skybox = runtimeMaterial;
        }

        return true;
    }

    private void TryRestoreLastSkybox()
    {
        string storedValue = PlayerPrefs.GetString(LastTextureKey, string.Empty);
        if (string.IsNullOrEmpty(storedValue))
        {
            return;
        }

        string resolvedPath = ResolveStoredPath(storedValue);
        if (string.IsNullOrEmpty(resolvedPath))
        {
            PlayerPrefs.DeleteKey(LastTextureKey);
            return;
        }

        if (!File.Exists(resolvedPath))
        {
            Debug.LogWarning("[SkyboxController] Stored skybox file missing: " + resolvedPath);
            PlayerPrefs.DeleteKey(LastTextureKey);
            return;
        }

        StartCoroutine(LoadAndApply(resolvedPath, false));
    }

    private void RememberLastUsedPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        string normalizedPath;
        try
        {
            normalizedPath = Path.GetFullPath(fullPath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SkyboxController] Failed to cache skybox path: " + ex.Message);
            return;
        }

        string value = "abs:" + normalizedPath;
        string streamingPath = Application.streamingAssetsPath;

        if (!string.IsNullOrEmpty(streamingPath))
        {
            try
            {
                var normalizedStreaming = Path.GetFullPath(streamingPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (normalizedPath.StartsWith(normalizedStreaming, StringComparison.OrdinalIgnoreCase))
                {
                    string relative = normalizedPath.Substring(normalizedStreaming.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    value = "rel:" + relative.Replace(Path.DirectorySeparatorChar, '/');
                }
            }
            catch (Exception)
            {
                // ignore normalization failure, fall back to absolute
            }
        }

        PlayerPrefs.SetString(LastTextureKey, value);
        PlayerPrefs.Save();
    }

    private string ResolveStoredPath(string storedValue)
    {
        if (string.IsNullOrEmpty(storedValue))
        {
            return null;
        }

        if (storedValue.StartsWith("rel:", StringComparison.Ordinal))
        {
            string relative = storedValue.Substring(4).TrimStart('/', '\\');
            string basePath = Application.streamingAssetsPath;
            if (string.IsNullOrEmpty(basePath))
            {
                return null;
            }

            return Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        if (storedValue.StartsWith("abs:", StringComparison.Ordinal))
        {
            return storedValue.Substring(4);
        }

        return storedValue;
    }

    private bool TryNormalizeFullPath(string path, out string normalizedPath)
    {
        normalizedPath = null;

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[SkyboxController] Path is empty.");
            return false;
        }

        string stripped = StripFileUriPrefix(path);

        try
        {
            normalizedPath = Path.GetFullPath(stripped);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SkyboxController] Path normalization failed: " + stripped + " (" + ex.Message + ")");
            return false;
        }
    }

    private static string StripFileUriPrefix(string path)
    {
        const string prefix = "file://";
        if (!string.IsNullOrEmpty(path) && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(prefix.Length);
        }

        return path;
    }
}
