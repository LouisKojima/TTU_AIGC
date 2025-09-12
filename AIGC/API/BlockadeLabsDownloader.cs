using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using IAV.AIGC.UI;

namespace IAV.AIGC.API
{
    // 负责下载 AIGC 生成的图片资源到本地（默认 StreamingAssets/SkyboxPreviews）
    public class BlockadeLabsDownloader : MonoBehaviour
    {
        [Header("Save Path")]
        [SerializeField] private bool _saveUnderStreamingAssets = true;
        [SerializeField] private string _streamingSubfolder = "SkyboxPreviews";
        [SerializeField] private string _absoluteDirectory = string.Empty; // 当不使用 StreamingAssets 时生效

        [Header("Options")]
        [SerializeField] private bool _overwriteExisting = true;

        [Header("UI Hooks (Optional)")]
        [SerializeField] private DynamicScrollPager _scrollPagerToRefresh;

        public event Action<string> OnDownloaded; // fullPath

        [Header("Last Download (Readonly)")]
        [SerializeField] private string _lastSavedPath;
        [SerializeField] [TextArea] private string _lastError;

        public void DownloadByUrl(string url)
        {
            StartCoroutine(DownloadCoroutine(url, null));
        }

        public void DownloadByUrlWithName(string url, string suggestedFileName)
        {
            StartCoroutine(DownloadCoroutine(url, suggestedFileName));
        }

        private IEnumerator DownloadCoroutine(string url, string suggestedFileName)
        {
            _lastSavedPath = null;
            _lastError = null;

            if (string.IsNullOrWhiteSpace(url))
            {
                _lastError = "URL 为空";
                Debug.LogWarning("[BlockadeLabsDownloader] URL 为空");
                yield break;
            }

            using (var req = UnityWebRequest.Get(url))
            {
                Debug.Log($"[BlockadeLabsDownloader] Start download: {url}");
                req.downloadHandler = new DownloadHandlerBuffer();
                yield return req.SendWebRequest();

                bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
                if (!ok)
                {
                    _lastError = $"下载失败: code={req.responseCode} error={req.error}";
                    Debug.LogWarning($"[BlockadeLabsDownloader] 下载失败: code={req.responseCode} error={req.error}");
                    yield break;
                }

                byte[] data = req.downloadHandler.data;
                if (data == null || data.Length == 0)
                {
                    _lastError = "下载数据为空";
                    Debug.LogWarning("[BlockadeLabsDownloader] 下载数据为空");
                    yield break;
                }

                string dir = ResolveSaveDirectory();
                try
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    _lastError = $"创建目录失败: {ex.Message}";
                    Debug.LogError($"[BlockadeLabsDownloader] 创建目录失败: {ex.Message}");
                    yield break;
                }

                string fileName = BuildFileName(url, suggestedFileName);
                string fullPath = Path.Combine(dir, fileName);

                if (File.Exists(fullPath) && !_overwriteExisting)
                {
                    // 为避免覆盖，追加时间戳
                    string name = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    fullPath = Path.Combine(dir, name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext);
                }

                try
                {
                    File.WriteAllBytes(fullPath, data);
                    _lastSavedPath = fullPath;
                    Debug.Log($"[BlockadeLabsDownloader] 保存成功: {fullPath}");

                    // 下载完成后切换状态为 Generated
                    SkyboxPopupStateController.SetState(IAV.AIGC.AIGCCreateState.Generated);

                    // 通知 UI 刷新（可选）
                    if (_scrollPagerToRefresh != null)
                    {
                        _scrollPagerToRefresh.Refresh();
                    }

                    // 事件回调
                    OnDownloaded?.Invoke(fullPath);
                }
                catch (Exception ex)
                {
                    _lastError = $"写入文件失败: {ex.Message}";
                    Debug.LogError($"[BlockadeLabsDownloader] 写入文件失败: {ex.Message}");
                }
            }
        }

        private string ResolveSaveDirectory()
        {
            if (_saveUnderStreamingAssets)
            {
                return Path.Combine(Application.streamingAssetsPath, _streamingSubfolder ?? string.Empty);
            }
            return string.IsNullOrWhiteSpace(_absoluteDirectory) ? Application.dataPath : _absoluteDirectory;
        }

        private static string BuildFileName(string url, string suggestedFileName)
        {
            string fileName = null;
            if (!string.IsNullOrWhiteSpace(suggestedFileName))
            {
                fileName = suggestedFileName;
            }
            else
            {
                try
                {
                    var uri = new Uri(url);
                    fileName = Path.GetFileName(uri.AbsolutePath);
                }
                catch
                {
                    // 忽略，走默认
                }
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "skybox_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            }

            fileName = SanitizeFileName(fileName);
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                // 无扩展名时，默认 .png
                fileName += ".png";
            }
            return fileName;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (var ch in name)
            {
                if (Array.IndexOf(invalid, ch) >= 0)
                    sb.Append('_');
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_saveUnderStreamingAssets && string.IsNullOrWhiteSpace(_streamingSubfolder))
            {
                _streamingSubfolder = "SkyboxPreviews";
            }
        }
#endif
    }
}
