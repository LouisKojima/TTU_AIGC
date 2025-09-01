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
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Sirenix.OdinInspector;
using System.Reflection;
using System.Collections.Generic;
using Unity.VisualScripting;
using static ConversationListCtrl.Side;
using Newtonsoft.Json;

public class ChineseRecognizer : MonoBehaviour
{
    private string appId = "31521874";
    private string appKey = "EMGxp0mmk49jiWjm1QZpGjrG";
    //private string devPid = "15372";      // Chinese
    private string devPid = "1737";      // English
    private string uri = "ws://vop.baidu.com/realtime_asr";
    private WebSocket websocket;

    private int sampleRate = 16000;
    private AudioClip recording;

    [FoldoutGroup("Refs")]
    public NLPAnalyzer recognition;
    //public TextRecognition textRecognition;
    [FoldoutGroup("Refs")]
    public List<ConversationListCtrl> conversationCtrls;
    [FoldoutGroup("Refs")]
    public Image volumeActionImg;
    [FoldoutGroup("Refs")]
    public BaiduTTS baiduTTS;
    [FoldoutGroup("Refs")]
    public AudioPlayer audioPlayer;
    [FoldoutGroup("Refs")]
    public Animator avatarAnim;

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
    [ShowInInspector]
    [DisplayAsString]
    private string receivedResult = "";
    [DisplayAsString]
    public float volume;
    [ShowInInspector]
    [DisplayAsString]
    private bool hasReply = false;
    private bool hasReplyOld = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        silentChunks = startSilentChunks;

        receivedMsg.Clear();
        receivedResult = "";
        //if (recognizedText) recognizedText.text = "Please say something.";
        //conversationCtrls.RemoveAllConversations();
        conversationCtrls.ForEach(x => x.RemoveAllConversations());
        conversationCtrls.ForEach(x => x.AddConversation("How can I help you?", Left));
        // audioPlayer.PlayAudioClip("Assets/IAV/HowCanIHelpYou.mp3");
        conversationCtrls.ForEach(x => x.AddConversation("", Right));
        avatarAnim.Play("Listen");
        //if (replyText) replyText.enabled = false;

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
            if(receivedMsg.result != "" && receivedMsg.result != null)
            {
                receivedResult = receivedMsg.result;
            }
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

    [ShowInInspector]
    bool voiceOutFlag = false;

    [Button]
    public async Task VoiceOut(string text)
    {
        voiceOutFlag = false;
        string resultFilePath = await Task<string>.Factory.StartNew(() => 
        {
            Debug.Log("Waiting for the audio...");
            return baiduTTS.SendTextToBaiduAPI(text);
        });
        Debug.Log("Audio downloaded at: " + resultFilePath);
        CustomEvent.Trigger(gameObject, "ReplyStart", text);
        audioPlayer.PlayAudioClip(resultFilePath);
        voiceOutFlag = true;
        return;
    }

    private bool activeInHierarchy => gameObject.activeInHierarchy;

    public async void ProcessRequest()
    {
        string reason = await Task<string>.Factory.StartNew(() =>
        {
            Debug.Log("Waiting for the LLM...");
            return recognition.TotalProcess(receivedResult);
        }); 
        reason = reason.Trim('\n', '\r', ' ');
        Debug.Log("Received reason:" + reason);
        if (gameObject.activeInHierarchy)
        {
            await VoiceOut(reason);
        }
        //await Task.Factory.StartNew(() => 
        //{
        //    VoiceOut(reason);
        //    while (!voiceOutFlag)
        //    {
        //        if (!activeInHierarchy) break;
        //        Task.Delay(100);
        //    }
        //    return;
        //});
        conversationCtrls.ForEach(x => x.SetLastText(reason));
        Debug.Log("reason: " + reason);
        recognition.DoAction();
    }

    private void Update()
    {
        if (receivedResult != "")
        {
            //if (hasReply && !hasReplyOld)
            //{
            //    conversationCtrls.ForEach(x => x.AddConversation(receivedResult, Right));
            //}
            //if (hasReply && hasReplyOld)
            if (hasReply)
            {
                conversationCtrls.ForEach(x => x.SetLastText(receivedResult));
            }
        }
        //if (receivedMsg.type == BDMessage.BDMessageType.FIN_TEXT)
        if (silentChunks <= 0)
        {
            avatarAnim.Play("Yes");
            conversationCtrls.ForEach(x => x.AddConversation("Analyzing ...", Left));
            hasReply = false;
            CloseWebSocket();
            silentChunks = startSilentChunks;
            Debug.Log("ASR voice input finish");
            receivedMsg.type = BDMessage.BDMessageType.INVALID;

            //Debug.Log("GPT Start analyzing : " + receivedResult);

            //textRecognition.Recognize();

            ProcessRequest();

            //string reason = recognition.TotalProcess(receivedResult);
            //reason = reason.Trim('\n', '\r', ' ');
            //conversationCtrls.ForEach(x => x.SetLastText(reason));
            //VoiceOut(reason);
            //Debug.Log("reason: " + reason);
            //recognition.DoAction();

            //if (textRecognition.reply != "")
            //{
            //    replyText.enabled = true;
            //    replyText.text = textRecognition.reply;
            //    if (hasReply)
            //    {
            //        VoiceOut();
            //        hasReply = false;
            //    }

            //}
        }
        if (volumeActionImg != null)
        {
            Color toSet = volumeActionImg.color;
            toSet.a = (float)(1 - volume) * 0.8f;
            volumeActionImg.color = toSet;
            //volumeActionImg.color = new(volumeActionImg.color.r, volumeActionImg.color.g, volumeActionImg.color.b, 1 - volume);
        }
        hasReplyOld = hasReply;
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
