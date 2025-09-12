using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using PusherClient;
using UnityEngine;
using IAV.AIGC.UI;
using System.IO;

namespace IAV.AIGC.API
{
    // 婵炶揪缍€濞夋洟寮?Pusher 闁荤姾娅ｉ崰鏇㈡煂?Blockade Labs 闂佹眹鍨婚崰鎰板垂濮橆厽浜ゆ繛鎴灻濠囨煥濞戞ɑ婀伴柣妤佹尦楠炴垿顢欓幆顬箓鏌熼璺ㄧ獢闁崇懓绉瑰畷?UI
    public class BlockadeLabsPusherTracker : MonoBehaviour
    {
        [Header("Pusher Config")]
        [SerializeField] private string _pusherKey = "a6a7b7662238ce4494d5";
        [SerializeField] private string _pusherCluster = "mt1";
        [SerializeField] private bool _connectOnStart = false;
        [SerializeField] private bool _autoSubscribeOnCreate = true;
        [SerializeField] private float _subscribeWaitTimeout = 3f;
        [SerializeField] private float _subscribeWaitInterval = 0.1f;

        [Header("References (Optional)")]
        [SerializeField] private BlockadeLabsSkyboxApi _api;
        [SerializeField] private BlockadeLabsDownloader _downloader;

        [Header("Debug")]
        [SerializeField] private string _currentChannelName;
        [SerializeField] private string _currentEventName = "status_update";
        [SerializeField] [TextArea] private string _lastEventPayload;
        [SerializeField] private string _lastStatus;

        private Pusher _pusher;
        private Channel _channel;
        private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
        private SynchronizationContext _unityContext;

        public event Action<string, string> OnStatusChanged; // (status, payload)
        public event Action<string> OnCompleted; // payload
        public event Action<string> OnErrored;   // payload

        private void Awake()
        {
            _unityContext = SynchronizationContext.Current;
        }

        private async void Start()
        {
            if (_connectOnStart)
            {
                await EnsureConnected();
                SubscribeUsingApiLastResponse();
            }
        }

        private void OnEnable()
        {
            if (_api != null && _autoSubscribeOnCreate)
            {
                _api.OnCreateAccepted += HandleCreateAccepted;
            }
        }

        private void OnDisable()
        {
            if (_api != null && _autoSubscribeOnCreate)
            {
                _api.OnCreateAccepted -= HandleCreateAccepted;
            }
        }

        private async void HandleCreateAccepted(long id, string channel, string ev)
        {
            await Subscribe(channel, ev);
        }

        public void SubscribeUsingApiLastResponseWithWait()
        {
            if (_api == null)
            {
                Debug.LogWarning("[BlockadeLabsPusherTracker] API reference not set; cannot auto-subscribe.");
                return;
            }
            StartCoroutine(WaitForApiPusherInfoAndSubscribe(_subscribeWaitTimeout));
        }

        private async Task EnsureConnected()
        {
            if (_pusher != null) return;
            if (string.IsNullOrWhiteSpace(_pusherKey) || string.IsNullOrWhiteSpace(_pusherCluster))
            {
                Debug.LogError("[BlockadeLabsPusherTracker] Pusher key/cluster not configured.");
                return;
            }

            _pusher = new Pusher(_pusherKey, new PusherOptions
            {
                Cluster = _pusherCluster,
                Encrypted = true
            });

            _pusher.Error += (s, e) => EnqueueToMain(() => Debug.LogWarning($"[BlockadeLabsPusherTracker] Pusher Error: {e.Message}"));
            _pusher.ConnectionStateChanged += (s, state) => EnqueueToMain(() => Debug.Log($"[BlockadeLabsPusherTracker] State: {state}"));
            _pusher.Connected += (s) => EnqueueToMain(() => Debug.Log("[BlockadeLabsPusherTracker] Connected"));

            await _pusher.ConnectAsync();
        }

        public async Task Subscribe(string channelName, string eventName)
        {
            await EnsureConnected();
            if (_pusher == null) return;

            _currentChannelName = channelName;
            _currentEventName = string.IsNullOrEmpty(eventName) ? "status_update" : eventName;

            _channel = await _pusher.SubscribeAsync(channelName);
            _pusher.Subscribed += (s, ch) =>
            {
                if (ch.Name == channelName)
                {
                    EnqueueToMain(() => Debug.Log($"[BlockadeLabsPusherTracker] Subscribed: {channelName}"));
                }
            };

            _channel.Bind(_currentEventName, (dynamic data) =>
            {
                string payload = data is string ? (string)data : data?.ToString();
                _lastEventPayload = payload;
                string dataJson = ExtractDataJson(payload);
                var status = ExtractStatus(dataJson ?? payload);
                _lastStatus = status;
                EnqueueToMain(() => OnStatusChanged?.Invoke(status, payload));
                EnqueueToMain(() => Debug.Log($"[BlockadeLabsPusherTracker] STATUS CHECK!!! status={status}"));

                if (string.IsNullOrEmpty(status)) return;
                var stLower = status.ToLowerInvariant();
                if (stLower == "complete")
                {
                    EnqueueToMain(() => { Debug.Log("[BlockadeLabsPusherTracker] DETECTED COMPLETE!!! Starting download from payload."); StartCoroutine(TryDownloadFromPayloadOrApi(payload)); });
                    EnqueueToMain(() => OnCompleted?.Invoke(payload));
                }
                else if (stLower == "pending" || stLower == "dispatched" || stLower == "processing")
                {
                    EnqueueToMain(() => SkyboxPopupStateController.SetState(IAV.AIGC.AIGCCreateState.Generating));
                }
                else if (stLower == "abort" || stLower == "error")
                {
                    EnqueueToMain(() => SkyboxPopupStateController.SetState(IAV.AIGC.AIGCCreateState.NotGenerated));
                    EnqueueToMain(() => OnErrored?.Invoke(payload));
                }
            });
        }

        public async void SubscribeUsingApiLastResponse()
        {
            if (_api == null)
            {
                Debug.LogWarning("[BlockadeLabsPusherTracker] API reference not set; cannot auto-subscribe.");
                return;
            }
            var channel = _api.GetLastPusherChannel();
            var ev = _api.GetLastPusherEvent();
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(ev))
            {
                Debug.LogWarning("[BlockadeLabsPusherTracker] Missing pusher_channel or pusher_event in API response.");
                return;
            }
            await Subscribe(channel, ev);
        }

        private static string ExtractStatus(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                // 缂備胶濮崑鎾绘煕濡や焦绀夌悮娆撴煛鐎ｎ偄濮х紒杈ㄧ箞瀵濡烽敃鈧ˉ?"status":"..."
                var idx = json.IndexOf("\"status\"", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return null;
                idx = json.IndexOf(':', idx);
                if (idx < 0) return null;
                var start = json.IndexOf('"', idx + 1);
                if (start < 0) return null;
                var end = json.IndexOf('"', start + 1);
                if (end < 0) return null;
                return json.Substring(start + 1, end - start - 1);
            }
            catch
            {
                return null;
            }
        }

        private static string ExtractUrl(string json)
        {
            // 闁诲繐绻戠换鍡涙儊椤栨稓顩?JSON 闂佸搫鍊稿ú锕€锕㈤悧鍫⑩枖妞ゆ挾鍠庣徊褰掓煕濞嗘劗澧柣鏍у閹插瓨鎷呴崫銉︽喕濠电偛鐗忛。顔炬濡惧”le_url / image_url / url闂?            if (string.IsNullOrEmpty(json)) return null;
            string url = TryExtractStringValue(json, "file_url");
            if (!string.IsNullOrEmpty(url)) return url;
            url = TryExtractStringValue(json, "image_url");
            if (!string.IsNullOrEmpty(url)) return url;
            url = TryExtractStringValue(json, "url");
            return url;
        }

        private static string TryExtractStringValue(string json, string key)
        {
            try
            {
                // 闂佸搫琚崕鍙夌珶?"key":"..."
                var keyToken = "\"" + key + "\"";
                int idx = json.IndexOf(keyToken, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return null;
                idx = json.IndexOf(':', idx);
                if (idx < 0) return null;
                int start = json.IndexOf('"', idx + 1);
                if (start < 0) return null;
                int end = json.IndexOf('"', start + 1);
                if (end < 0) return null;
                return json.Substring(start + 1, end - start - 1);
            }
            catch { return null; }
        }

        private IEnumerator TryDownloadFromPayloadOrApi(string payload)
        {
    if (_downloader == null)
    {
        Debug.LogWarning("[BlockadeLabsPusherTracker] Downloader reference not set; cannot auto-download.");
        yield break;
    }

    // Try to extract file_url from payload's data JSON block
    string dataJson = ExtractDataJson(payload);
    var url = ExtractUrl(dataJson ?? payload);
    if (!string.IsNullOrEmpty(url))
    {
        url = UnescapeJsonUrl(url);
        string suggested = BuildSuggestedFileName(url, dataJson);
        Debug.Log($"[BlockadeLabsPusherTracker] Payload URL extracted, start download: {url}"); _downloader.DownloadByUrlWithName(url, suggested);
        yield break;
    }

    // Fallback: query status by last create id, then extract URL
    if (_api != null && _api.GetLastCreateId() > 0)
    {
        Debug.Log("[BlockadeLabsPusherTracker] Fallback: querying API status for URL..."); yield return _api.GetSkyboxStatus(_api.GetLastCreateId());
        var statusJson = _api.GetLastStatusResponseJson();
        url = ExtractUrl(statusJson); Debug.Log($"[BlockadeLabsPusherTracker] Fallback status URL parsed: {url}");
        if (!string.IsNullOrEmpty(url))
        {
            url = UnescapeJsonUrl(url);
            string suggested = BuildSuggestedFileName(url, statusJson);
            Debug.Log($"[BlockadeLabsPusherTracker] Payload URL extracted, start download: {url}"); _downloader.DownloadByUrlWithName(url, suggested);
        }
        else
        {
            Debug.LogWarning("[BlockadeLabsPusherTracker] Could not extract image URL from status response.");
        }
    }
    else
    {
        Debug.LogWarning("[BlockadeLabsPusherTracker] Missing API or last create id; cannot fallback status query.");
    }
}

        private IEnumerator WaitForApiPusherInfoAndSubscribe(float timeoutSeconds)
        {
            float elapsed = 0f;
            while (elapsed < timeoutSeconds)
            {
                var channel = _api != null ? _api.GetLastPusherChannel() : null;
                var ev = _api != null ? _api.GetLastPusherEvent() : null;
                if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(ev))
                {
                    _ = Subscribe(channel, ev);
                    yield break;
                }
                yield return new WaitForSeconds(_subscribeWaitInterval);
                elapsed += _subscribeWaitInterval;
            }
            Debug.LogWarning("[BlockadeLabsPusherTracker] Timeout waiting for pusher_channel/pusher_event.");
        }
        private void EnqueueToMain(Action action)
        {
            if (action == null) return;
            if (SynchronizationContext.Current == _unityContext)
            {
                action();
            }
            else
            {
                _mainThreadQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            while (_mainThreadQueue.TryDequeue(out var a))
            {
                try { a(); }
                catch (Exception ex) { Debug.LogWarning($"[BlockadeLabsPusherTracker] Dispatch error: {ex.Message}"); }
            }
        }
                private static string UnescapeJsonUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            return url.Replace("\\/", "/");
        }

        private static string SanitizeForFile(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if (System.Array.IndexOf(invalid, ch) >= 0) sb.Append('_');
                else sb.Append(ch);
            }
            return sb.ToString().Replace(' ', '-');
        }

        private static string TryExtractNumericValue(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return null;
            try
            {
                var keyToken = "\"" + key + "\"";
                int idx = json.IndexOf(keyToken, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return null;
                idx = json.IndexOf(':', idx);
                if (idx < 0) return null;
                int i = idx + 1;
                while (i < json.Length && (json[i] == ' ' || json[i] == '"')) i++;
                int start = i;
                while (i < json.Length && char.IsDigit(json[i])) i++;
                if (i > start) return json.Substring(start, i - start);
                return null;
            }
            catch { return null; }
        }

        private static string BuildSuggestedFileName(string url, string json)
        {
            string id = TryExtractNumericValue(json ?? string.Empty, "id");
            if (string.IsNullOrEmpty(id))
            {
                try
                {
                    var u = new System.Uri(url);
                    var name = System.IO.Path.GetFileName(u.AbsolutePath);
                    int us = name.LastIndexOf('_');
                    int dot = name.LastIndexOf('.');
                    if (us >= 0 && dot > us)
                    {
                        var maybe = name.Substring(us+1, dot-us-1);
                        int tmp; if (int.TryParse(maybe, out tmp)) id = maybe;
                    }
                }
                catch { }
            }
            if (string.IsNullOrEmpty(id)) id = "unknown";

            string style = TryExtractStringValue(json ?? string.Empty, "skybox_style_name");
            if (string.IsNullOrEmpty(style)) style = TryExtractStringValue(json ?? string.Empty, "model");
            if (string.IsNullOrEmpty(style)) style = "style";
            string styleSlug = SanitizeForFile(style).ToLowerInvariant();

            string ext = null;
            try
            {
                var u = new System.Uri(url);
                ext = System.IO.Path.GetExtension(u.AbsolutePath);
            }
            catch { }
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";

            string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"skybox_{id}_{styleSlug}_{ts}{ext}";
        }

        private static string ExtractDataJson(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return null;
            int idx = payload.IndexOf("data = {", System.StringComparison.OrdinalIgnoreCase);
            int offset = 8; // length of 'data = '
            if (idx < 0)
            {
                idx = payload.IndexOf("\"data\"", System.StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    idx = payload.IndexOf('{', idx);
                    offset = 0;
                }
            }
            if (idx < 0) return null;
            int start = payload.IndexOf('{', idx + offset);
            if (start < 0) return null;
            int depth = 0;
            for (int i = start; i < payload.Length; i++)
            {
                char c = payload[i];
                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return payload.Substring(start, i - start + 1);
                    }
                }
            }
            return null;
        }private async void OnApplicationQuit()
        {
            if (_pusher != null)
            {
                await _pusher.DisconnectAsync();
            }
        }
    }

}
