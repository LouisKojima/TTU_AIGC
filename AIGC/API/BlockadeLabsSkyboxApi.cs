using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace IAV.AIGC.API
{
    // 负责与 Blockade Labs Skybox API 通信：创建、查询状态、保存返回信息（含 id、pusher 通道信息）
    public class BlockadeLabsSkyboxApi : MonoBehaviour
    {
        [Header("API Config")]
        [Tooltip("API 基础地址，例如 https://api.blockadelabs.com/v1")]
        [SerializeField] private string _apiBaseUrl = "https://api.blockadelabs.com/v1";

        [Tooltip("在 Blockade Labs 个人设置中获取的 x-api-key")]
        [SerializeField] private string _apiKey;

        [Tooltip("请求超时时间（秒）")]
        [SerializeField] private int _timeoutSeconds = 60;

        [Header("Defaults")]
        [Tooltip("默认风格 ID（示例：67 = M3 Photoreal）")]
        [SerializeField] private int _defaultSkyboxStyleId = 67;

        [Tooltip("默认 negative_text（可留空）")]
        [SerializeField] [TextArea] private string _defaultNegativeText = string.Empty;

        [Header("Last Response Snapshot (Readonly)")]
        [SerializeField] [TextArea] private string _lastCreateResponseJson;
        [SerializeField] private long _lastCreateId;
        [SerializeField] [TextArea] private string _lastStatusResponseJson;
        [SerializeField] private string _lastPusherChannel;
        [SerializeField] private string _lastPusherEvent;

        public event Action<long, string, string> OnCreateAccepted; // (id, channel, event)

        [Serializable]
        private class CreateRequest
        {
            public string prompt;
            public string negative_text;
            public int skybox_style_id;
        }

        [Serializable]
        private class CreateResponse
        {
            public long id;
            public string pusher_channel;
            public string pusher_event;
        }

        // 外部调用：按默认配置创建天空盒
        public Coroutine CreateSkybox(string prompt)
        {
            return CreateSkybox(prompt, _defaultNegativeText, _defaultSkyboxStyleId);
        }

        // 外部调用：自定义 negative_text 与 styleId
        public Coroutine CreateSkybox(string prompt, string negativeText, int styleId)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Debug.LogError("[BlockadeLabsSkyboxApi] x-api-key 未配置。");
                return null;
            }

            var payload = new CreateRequest
            {
                prompt = prompt,
                negative_text = string.IsNullOrEmpty(negativeText) ? null : negativeText,
                skybox_style_id = styleId
            };
            var json = JsonUtility.ToJson(payload);
            var url = CombineUrl(_apiBaseUrl, "/skybox");

            return StartCoroutine(PostJson(url, json, (ok, resp) =>
            {
                _lastCreateResponseJson = resp;
                if (!ok)
                {
                    Debug.LogWarning("[BlockadeLabsSkyboxApi] Create 失败: " + resp);
                    return;
                }

                try
                {
                    var parsed = JsonUtility.FromJson<CreateResponse>(resp);
                    if (parsed != null)
                    {
                        if (parsed.id > 0)
                        {
                            _lastCreateId = parsed.id;
                            Debug.Log($"[BlockadeLabsSkyboxApi] Create 提交成功，id={parsed.id}");
                        }
                        else
                        {
                            Debug.LogWarning("[BlockadeLabsSkyboxApi] 未在返回中解析到 id。");
                        }

                        _lastPusherChannel = parsed.pusher_channel;
                        _lastPusherEvent = parsed.pusher_event;
                        if (!string.IsNullOrEmpty(_lastPusherChannel) && !string.IsNullOrEmpty(_lastPusherEvent))
                        {
                            Debug.Log($"[BlockadeLabsSkyboxApi] Pusher: channel={_lastPusherChannel}, event={_lastPusherEvent}");
                            OnCreateAccepted?.Invoke(_lastCreateId, _lastPusherChannel, _lastPusherEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BlockadeLabsSkyboxApi] 解析创建响应失败: {ex.Message}");
                }
            }));
        }

        // 外部调用：查询状态（一次）
        public Coroutine GetSkyboxStatus(long id)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Debug.LogError("[BlockadeLabsSkyboxApi] x-api-key 未配置。");
                return null;
            }
            var url = CombineUrl(_apiBaseUrl, $"/skybox/{id}");
            return StartCoroutine(GetJson(url, (ok, resp) =>
            {
                _lastStatusResponseJson = resp;
                if (!ok)
                {
                    Debug.LogWarning($"[BlockadeLabsSkyboxApi] GetStatus 失败: {resp}");
                }
            }));
        }

        // 外部调用：按固定间隔轮询，直到状态为完成或错误（简单示例，未强依赖具体返回结构）
        public Coroutine PollStatusUntilComplete(long id, float intervalSeconds = 3f)
        {
            return StartCoroutine(PollRoutine(id, intervalSeconds));
        }

        private IEnumerator PollRoutine(long id, float intervalSeconds)
        {
            while (true)
            {
                yield return GetSkyboxStatus(id);

                // 粗略判定：包含 "complete" 认为完成；包含 "failed"/"error" 认为失败
                var text = _lastStatusResponseJson ?? string.Empty;
                var lower = text.ToLowerInvariant();
                if (lower.Contains("\"status\":\"complete\"") || lower.Contains("\"state\":\"complete\""))
                {
                    Debug.Log("[BlockadeLabsSkyboxApi] 生成完成。");
                    yield break;
                }
                if (lower.Contains("failed") || lower.Contains("error"))
                {
                    Debug.LogWarning("[BlockadeLabsSkyboxApi] 生成失败或出错。");
                    yield break;
                }

                yield return new WaitForSeconds(intervalSeconds);
            }
        }

        // 发送 POST JSON
        private IEnumerator PostJson(string url, string json, Action<bool, string> completed)
        {
            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.timeout = Mathf.Max(1, _timeoutSeconds);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("x-api-key", _apiKey);

                yield return req.SendWebRequest();

                bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
                string resp = req.downloadHandler != null ? req.downloadHandler.text : null;
                if (!ok)
                {
                    Debug.LogWarning($"[BlockadeLabsSkyboxApi] POST {url} 失败: code={req.responseCode} error={req.error}\n{resp}");
                }
                completed?.Invoke(ok, resp);
            }
        }

        // 发送 GET JSON
        private IEnumerator GetJson(string url, Action<bool, string> completed)
        {
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = Mathf.Max(1, _timeoutSeconds);
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("x-api-key", _apiKey);

                yield return req.SendWebRequest();

                bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
                string resp = req.downloadHandler != null ? req.downloadHandler.text : null;
                if (!ok)
                {
                    Debug.LogWarning($"[BlockadeLabsSkyboxApi] GET {url} 失败: code={req.responseCode} error={req.error}\n{resp}");
                }
                completed?.Invoke(ok, resp);
            }
        }

        private static string CombineUrl(string baseUrl, string path)
        {
            if (string.IsNullOrEmpty(baseUrl)) return path ?? string.Empty;
            if (string.IsNullOrEmpty(path)) return baseUrl;
            if (baseUrl.EndsWith("/")) baseUrl = baseUrl.TrimEnd('/');
            if (!path.StartsWith("/")) path = "/" + path;
            return baseUrl + path;
        }

        // 便于在 Inspector 的按钮绑定中直接调用的简化方法
        public void CreateWithInspectorDefaults(string prompt)
        {
            CreateSkybox(prompt);
        }

        public long GetLastCreateId() => _lastCreateId;
        public string GetLastCreateResponseJson() => _lastCreateResponseJson;
        public string GetLastStatusResponseJson() => _lastStatusResponseJson;
        public string GetLastPusherChannel() => _lastPusherChannel;
        public string GetLastPusherEvent() => _lastPusherEvent;
    }
}
