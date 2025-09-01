using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json.Linq;

public class BaiduTTS : MonoBehaviour
{
    const string API_KEY = "EMGxp0mmk49jiWjm1QZpGjrG";
    const string SECRET_KEY = "teBGA7PeBfqfnLH0cMj2nhwcRPApGIfc";
    public string TEXT = "The ambient light has been changed to yellow";

    // 发音人选择, 基础音库：0为度小美，1为度小宇，3为度逍遥，4为度丫丫，
    // 精品音库：5为度小娇，103为度米朵，106为度博文，110为度小童，111为度小萌，默认为度小美
    const int PER = 0;
    //语速，取值0-15，默认为5中语速
    const int SPD = 5;
    //音调，取值0-15，默认为5中语调
    const int PIT = 5;
    //音量，取值0-9，默认为5中音量
    const int VOL = 5;
    //下载的文件格式, 3：mp3(default) 4： pcm-16k 5： pcm-8k 6. wav
    const int AUE = 6;
    const string CUID = "123456UNITY";

    const string TTS_URL = "http://tsn.baidu.com/text2audio";
    [ShowInInspector, DisplayAsString]
    private string persistentDataPath;

    private void Awake()
    {
        persistentDataPath = Application.persistentDataPath;
    }

    static JObject ParseJson(string json)
    {
        JObject tokenData = JObject.Parse(json);
        return tokenData;
    }

    static string FetchToken()
    {
        string tokenUrl = "http://aip.baidubce.com/oauth/2.0/token";
        string scope = "audio_tts_post";

        string tokenRequestData = $"grant_type=client_credentials&client_id={API_KEY}&client_secret={SECRET_KEY}";

        using (var client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string tokenResponse = client.UploadString(tokenUrl, tokenRequestData);

            JObject tokenData = ParseJson(tokenResponse);
            
            if (tokenData.ContainsKey("access_token") && tokenData.ContainsKey("scope"))
            {
                string token = (string)tokenData.GetValue("access_token");
                string tokenScope = (string)tokenData.GetValue("scope");

                if (!tokenScope.Contains(scope))
                    throw new Exception("Scope is not correct");

                Debug.Log($"SUCCESS WITH TOKEN: {token}; EXPIRES IN SECONDS: {tokenData["expires_in"]}");
                return token;
            }
            else
            {
                throw new Exception("API_KEY or SECRET_KEY not correct: access_token or scope not found in token response");
            }
        }
    }

    IEnumerator Start()
    {
        string token = FetchToken();
        string encodedText = Uri.EscapeDataString(Uri.EscapeDataString(TEXT));
        string data = $"tok={token}&tex={encodedText}&per={PER}&spd={SPD}&pit={PIT}&vol={VOL}&aue={AUE}&cuid={CUID}&lan=zh&ctp=1";

        using (var client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            byte[] responseBytes = client.UploadData(TTS_URL, "POST", Encoding.UTF8.GetBytes(data));
            string contentType = client.ResponseHeaders[HttpResponseHeader.ContentType];

            if (!contentType.Contains("audio/"))
            {
                string errorResponse = Encoding.UTF8.GetString(responseBytes);
                Debug.LogError("TTS API error: " + errorResponse);
                yield break;
            }

            string saveFile = contentType.Contains("audio/mp3") ? "result.mp3" : "result.wav";
            string savePath = Path.Combine(persistentDataPath, saveFile);
            File.WriteAllBytes(savePath, responseBytes);

            Debug.Log("Result saved as: " + savePath);
        }
    }

    public string SendTextToBaiduAPI(string inputText)
    {
        // Replace the default TEXT value with inputText
        TEXT = inputText;

        // Call the existing logic with the new TEXT value
        string resultFilePath = SendTextToBaidu();

        // Return the result file path
        return resultFilePath;
    }

    string SendTextToBaidu()
    {
        string token = FetchToken();
        string encodedText = Uri.EscapeDataString(Uri.EscapeDataString(TEXT));
        string data = $"tok={token}&tex={encodedText}&per={PER}&spd={SPD}&pit={PIT}&vol={VOL}&aue={AUE}&cuid={CUID}&lan=zh&ctp=1";

        using (var client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            byte[] responseBytes = client.UploadData(TTS_URL, "POST", Encoding.UTF8.GetBytes(data));
            string contentType = client.ResponseHeaders[HttpResponseHeader.ContentType];

            if (!contentType.Contains("audio/"))
            {
                string errorResponse = Encoding.UTF8.GetString(responseBytes);
                Debug.LogError("TTS API error: " + errorResponse);
                return null;
            }

            string saveFile = contentType.Contains("audio/mp3") ? "result.mp3" : "result.wav";
            string savePath = Path.Combine(persistentDataPath, saveFile);
            File.WriteAllBytes(savePath, responseBytes);

            Debug.Log("Result saved as: " + savePath);

            return savePath;
        }
    }
}

