using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using NAudio.Wave;
// using UnityEngine.UI;
// using TMPro;
using Sirenix.OdinInspector;
using System.Reflection;
using System.Collections.Generic;
// using Unity.VisualScripting;
using Newtonsoft.Json;

public class ASRFromBaidu : MonoBehaviour
{
    private string appId = "31521874";
    private string appKey = "EMGxp0mmk49jiWjm1QZpGjrG";
    // 在 Inspector 中可切换识别语言（会映射到不同的 dev_pid）
    public enum AsrLanguage
    {
        Mandarin = 15372,
        English = 17372
    }
    [FoldoutGroup("Settings")]
    public AsrLanguage language = AsrLanguage.English;
    private string uri = "ws://vop.baidu.com/realtime_asr";
    private WebSocket websocket;

    private int sampleRate = 16000;
    private AudioClip recording;

    // LLM/NLP integration removed — pure ASR only
    //public TextRecognition textRecognition;
    // No UI references

    [FoldoutGroup("Settings")]
    public int silenceThreshold = 4000000;
    [FoldoutGroup("Settings")]
    public int endSlientChunks = 15;
    [FoldoutGroup("Settings")]
    public int startSilentChunks = 100;


    private int chunkSize = 1024;
    private int channels = 1;

    [DisplayAsString]
    public int silentChunks = 30;
    private bool finishSent = false;
    private readonly object transcriptLock = new();
    private readonly List<string> confirmedSegments = new();
    private readonly Dictionary<string, int> finalizedSnToIndex = new();
    private string currentPartial = string.Empty;
    public string AggregatedText { get; private set; } = string.Empty;
    // Current text can be read from receivedMsg.result by other modules
    // public float volume;
    // UI flags removed

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        silentChunks = startSilentChunks;

        ResetTranscriptState();

        receivedMsg.Clear();
        finishSent = false;
        

        ConnectWebSocket();
    }

    private void OnDisable()
    {
        OnDestroy();
    }

    // OnDestroy is called when the script is being destroyed
    private void OnDestroy()
    {
        CloseWebSocket();
    }

    [ShowInInspector, SerializeField]
    [CustomContextMenu("Clear", "ClearMsg")]
    public BDMessage receivedMsg = new();

    public void ClearMsg()
    {
        receivedMsg = new();
        ResetTranscriptState();
    }

    private void ResetTranscriptState()
    {
        lock (transcriptLock)
        {
            confirmedSegments.Clear();
            finalizedSnToIndex.Clear();
            currentPartial = string.Empty;
            AggregatedText = string.Empty;
        }
    }

    private void ConnectWebSocket()
    {
        websocket = new WebSocket(uri + "?sn=" + Guid.NewGuid().ToString());

        websocket.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected.");
            SendStartParams();
            StartCoroutine(SendAudio());
        };

        websocket.OnMessage += (sender, e) =>
        {
            Debug.Log("Received: " + e.Data);
            receivedMsg.Clear();
            receivedMsg.PraseString(e.Data);
            UpdateAggregatedTranscript();
        };

        websocket.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed.");
        };

        websocket.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
        };

        websocket.Connect();
    }

    // TTS / Audio playback removed for a pure ASR-only behaviour

    // private bool activeInHierarchy => gameObject.activeInHierarchy;

    // LLM/TTS流程移除，保留 ASR 文本（可由其他模块消费）

    private void UpdateAggregatedTranscript()
    {
        lock (transcriptLock)
        {
            switch (receivedMsg.type)
            {
                case BDMessage.BDMessageType.MID_TEXT:
                    currentPartial = receivedMsg.result ?? string.Empty;
                    break;
                case BDMessage.BDMessageType.FIN_TEXT:
                    string finalized = (receivedMsg.result ?? string.Empty).Trim();
                    string sn = receivedMsg.sn ?? string.Empty;
                    if (!string.IsNullOrEmpty(sn))
                    {
                        if (finalizedSnToIndex.TryGetValue(sn, out int existingIndex))
                        {
                            if (!string.IsNullOrEmpty(finalized))
                            {
                                confirmedSegments[existingIndex] = finalized;
                            }
                        }
                        else if (!string.IsNullOrEmpty(finalized))
                        {
                            finalizedSnToIndex[sn] = confirmedSegments.Count;
                            confirmedSegments.Add(finalized);
                        }
                    }
                    else if (!string.IsNullOrEmpty(finalized))
                    {
                        confirmedSegments.Add(finalized);
                    }
                    currentPartial = string.Empty;
                    break;
                default:
                    return;
            }

            AggregatedText = BuildAggregatedText();
        }
    }

    private string BuildAggregatedText()
    {
        if (confirmedSegments.Count == 0 && string.IsNullOrEmpty(currentPartial))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < confirmedSegments.Count; i++)
        {
            string segment = confirmedSegments[i];
            if (string.IsNullOrEmpty(segment))
            {
                continue;
            }
            sb.Append(segment);
        }

        if (!string.IsNullOrEmpty(currentPartial))
        {
            sb.Append(currentPartial);
        }

        return sb.ToString();
    }

    private void Update()
    {
        if (silentChunks <= 0)
        {
            CloseWebSocket();
            silentChunks = startSilentChunks;
            Debug.Log("ASR voice input finish");
            receivedMsg.type = BDMessage.BDMessageType.INVALID;
        }
    }

    //public string GetResult(string msgData)
    //{
    //    string result = "No result";
    //    string[] data = msgData.Replace("\"", "").Split(',');
    //    foreach (string s in data)
    //    {
    //        if (s.Contains("result"))
    //        {
    //            result = s;
    //            break;
    //        }
    //    }

    //    return result.Replace("result:", "");
    //}

    private void CloseWebSocket()
    {
        if (websocket != null && websocket.ReadyState == WebSocketState.Open)
        {
            websocket.Close();
            Debug.Log("WebSocket connection closed.");
        }
    }

    private void SendStartParams()
    {
        // Ensure numeric types where required by the API (appid/dev_pid/sample)
        long appIdNum = 0;
        if (!long.TryParse(appId, out appIdNum))
        {
            Debug.LogWarning($"[ChineseRecognizer] appId '{appId}' is not numeric. Using 0 as fallback.");
        }

        int devPidNum = (int)language;

        var req = new StartFrame
        {
            Type = "START",
            Data = new StartData
            {
                AppId = appIdNum,
                AppKey = appKey,
                DevPid = devPidNum,
                Cuid = "yourself_defined_user_id",
                Sample = sampleRate,
                Format = "pcm"
            }
        };

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        string body = JsonConvert.SerializeObject(req, jsonSettings);
        websocket.Send(body);
        Debug.Log("Sent START frame with params: " + body);
    }

    // Strongly-typed payloads to ensure correct JSON field names and types
    private class StartFrame
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("data")] public StartData Data { get; set; }
    }

    private class StartData
    {
        [JsonProperty("appid")] public long AppId { get; set; }
        [JsonProperty("appkey")] public string AppKey { get; set; }
        [JsonProperty("dev_pid")] public int DevPid { get; set; }
        [JsonProperty("cuid")] public string Cuid { get; set; }
        [JsonProperty("sample")] public int Sample { get; set; }
        [JsonProperty("format")] public string Format { get; set; }
    }


    private IEnumerator SendAudioFile()
    {
        string filePath = "C:\\Users\\18012\\VoiceRecognizer\\Assets\\output.pcm";
        int chunkMs = 160;
        int chunkLen = (int)(16000 * 2 / 1000 * chunkMs);

        if (!File.Exists(filePath))
        {
            Debug.LogError("PCM file not found: " + filePath);
            yield break;
        }

        byte[] pcm;
        try
        {
            pcm = File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading PCM file: " + e.Message);
            yield break;
        }

        int index = 0;
        int total = pcm.Length;
        Debug.Log("send_audio total=" + total);

        while (index < total)
        {
            int end = index + chunkLen;
            if (end >= total)
            {
                end = total;
            }
            byte[] body = new byte[end - index];
            Array.Copy(pcm, index, body, 0, end - index);

            Debug.Log("try to send audio length " + body.Length + ", from bytes [" + index + "," + end + ")");

            if (websocket.ReadyState == WebSocketState.Open)
            {
                websocket.Send(body);
            }
            else
            {
                Debug.LogError("WebSocket connection is not open.");
                break;
            }

            index = end;
            yield return new WaitForSeconds((float)chunkMs / 1000);
        }

        SendFinish();
    }

    private IEnumerator SendAudio()
    {
        int chunkMs = 160;
        int sampleRate = 16000;
        int channels = 1;
        int bytesPerSample = 2;
        int chunkSize = (int)(sampleRate * bytesPerSample / 1000 * chunkMs);

        using (WaveInEvent waveIn = new WaveInEvent())
        {
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
            waveIn.BufferMilliseconds = chunkMs;
            waveIn.NumberOfBuffers = 2;

            bool isRecording = true;
            waveIn.DataAvailable += (sender, e) =>
            {
                if (isRecording)
                {
                    // Add the silence detection functionality
                    float rms = CalculateRMS(e.Buffer, e.BytesRecorded);
                    //Debug.Log("RMS = " + rms);
                    if (rms < silenceThreshold)
                    {
                        if (--silentChunks <= 0)
                        {
                            isRecording = false;
                        }
                        Debug.Log("silentChunks = " + silentChunks);
                    }
                    else
                    {
                        silentChunks = endSlientChunks;
                    }

                    // Send audio data
                    if (isRecording)
                    {
                        byte[] body = new byte[e.BytesRecorded];
                        Array.Copy(e.Buffer, 0, body, 0, e.BytesRecorded);

                        Debug.Log("try to send audio length " + body.Length);

                        if (websocket.ReadyState == WebSocketState.Open)
                        {
                            websocket.Send(body);
                        }
                        else
                        {
                            Debug.LogError("WebSocket connection is not open.");
                            isRecording = false;
                        }
                    }
                    else
                    {
                        Debug.Log("Voice Silence End.");
                    }
                }
            };

            // Removed UI volume visualization hook (volumeActionImg)

            waveIn.StartRecording();
            while (isRecording)
            {
                yield return new WaitForSeconds((float)chunkMs / 1000);
            }
            waveIn.StopRecording();
        }

        SendFinish();
    }

    float CalculateRMS(byte[] buffer, int bytesRecorded)
    {
        float sum = 0;
        for (int i = 0; i < bytesRecorded; i += 2)
        {
            short sample = BitConverter.ToInt16(buffer, i);
            sum += sample * sample;
        }
        return Mathf.Sqrt(sum / (bytesRecorded / 2)) * 32767;
    }






    private void SendFinish()
    {
        var req = new
        {
            type = "FINISH"
        };
        string body = JsonConvert.SerializeObject(req);
        if (finishSent)
        {
            Debug.Log("[ASRFromBaidu] FINISH already sent; skip.");
            return;
        }
        if (websocket != null && websocket.ReadyState == WebSocketState.Open)
        {
            try
            {
                websocket.Send(body);
                finishSent = true;
                Debug.Log("Sent FINISH frame");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ASRFromBaidu] FINISH send failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("[ASRFromBaidu] FINISH skipped: WebSocket not open");
        }
    }
}
