using System;
using System.IO;
using System.Collections;
using UnityEngine;
using WebSocketSharp;
using NAudio.Wave;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Newtonsoft.Json;

public class SpeechRecognizer : MonoBehaviour
{
    private string appId = "31521874";
    private string appKey = "EMGxp0mmk49jiWjm1QZpGjrG";
    private string devPid = "17372";
    private string uri = "ws://vop.baidu.com/realtime_asr";
    private WebSocket websocket;

    private int sampleRate = 16000;
    private AudioClip recording;

    [FoldoutGroup("Refs")]
    public TextRecognition textRecognition;
    [FoldoutGroup("Refs")]
    public Text recognizedText;
    [FoldoutGroup("Refs")]
    public Text replyText;
    [FoldoutGroup("Refs")]
    public Image volumeActionImg;
    [FoldoutGroup("Refs")]
    public BaiduTTS baiduTTS;
    [FoldoutGroup("Refs")]
    public AudioPlayer audioPlayer;
    [FoldoutGroup("Refs")]
    public ConversationListCtrl conversationCtrl;
    [FoldoutGroup("Refs")]
    public ScriptMachine avatarAnimationScriptMachine;

    [FoldoutGroup("Settings")]
    public int silenceThreshold = 3000000;
    [FoldoutGroup("Settings")]
    public int endSlientChunks = 10;
    [FoldoutGroup("Settings")]
    public int startSilentChunks = 50;


    private int chunkSize = 1024;
    private int channels = 1;

    [DisplayAsString]
    public int silentChunks = 50;
    [ShowInInspector]
    [DisplayAsString]
    private string receivedResult = "No Result";
    [DisplayAsString]
    public float volume;
    [ShowInInspector]
    [DisplayAsString]
    private bool hasReply = false;

    // Start is called before the first frame update
    void Start()
    {
        silentChunks = startSilentChunks;

        receivedMsg.Clear();
        if (replyText)
        {
            replyText.enabled = false;
        }
        if (recognizedText)
        {
            recognizedText.text = "Please say something.";
        }
        if (conversationCtrl)
        {
            conversationCtrl.AddConversation("Can I help you?", ConversationListCtrl.Side.Left);
        }

        ConnectWebSocket();
    }

    private void OnEnable()
    {
        Start();
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
            receivedResult = receivedMsg.result;
            hasReply = true;
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

    [Button]
    public void VoiceOut()
    {
        CustomEvent.Trigger(avatarAnimationScriptMachine.gameObject, "ReplyStart", textRecognition.reply);
        string resultFilePath = baiduTTS.SendTextToBaiduAPI(textRecognition.reply);
        audioPlayer.PlayAudioClip(resultFilePath);
    }
    private void Update()
    {
        if (receivedResult != "")
        {
            if (recognizedText)
            {
                recognizedText.text = receivedResult;
            }
        }
        if (receivedMsg.type == BDMessage.BDMessageType.FIN_TEXT)
        {
            textRecognition.Recognize(receivedResult);

            if (conversationCtrl)
            {
                conversationCtrl.AddConversation(receivedResult, ConversationListCtrl.Side.Right);
                conversationCtrl.AddConversation(textRecognition.reply, ConversationListCtrl.Side.Left);
            }
            if (replyText)
            {
                replyText.enabled = true;
                replyText.text = textRecognition.reply;
            }
            if (hasReply)
            {
                VoiceOut();
                hasReply = false;
            }
            CloseWebSocket();
        }
        if (volumeActionImg)
        {
            Color toSet = volumeActionImg.color;
            toSet.a = (float)(1 - volume) * 0.8f;
            volumeActionImg.color = toSet;
            //volumeActionImg.color = new(volumeActionImg.color.r, volumeActionImg.color.g, volumeActionImg.color.b, 1 - volume);
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
        var req = new
        {
            type = "START",
            data = new
            {
                appid = appId,
                appkey = appKey,
                dev_pid = devPid,
                cuid = "yourself_defined_user_id",
                sample = sampleRate,
                format = "pcm"
            }
        };

        string body = JsonConvert.SerializeObject(req);
        string testBody = "{\"type\": \"START\", \"data\": {\"appid\": 31521874, \"appkey\": \"EMGxp0mmk49jiWjm1QZpGjrG\", \"dev_pid\": 17372, \"cuid\": \"yourself_defined_user_id\", \"sample\": 16000, \"format\": \"pcm\"}}";
        websocket.Send(testBody);
        Debug.Log("Sent START frame with params: " + body);
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
                    Debug.Log("RMS = " + rms);
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

            waveIn.DataAvailable += (sender, e) =>
            {
                short[] values = new short[e.Buffer.Length / 2];
                Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

                float max = (float)values.Max() / short.MaxValue;
                volume = max;
            };

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
        websocket.Send(body);
        Debug.Log("Sent FINISH frame");
    }
}
