using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;
using System.Threading;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// This version can establish the connection correctly
public class SparkLLM : MonoBehaviour
{

    const string appId = "d1fbf3c1";
    const string apiKey = "9b943ffbdb4950a7181b13f3e687aac6";
    const string apiSecret = "ZGU1NDRlOWQ4NzIzMmYyODMzNDBlY2Zj";
    const string hostUrl = "http://spark-api.xf-yun.com/v4.0/chat";
    const string prompt = "Suppose you are one AI voice assistant inside a car. Your task is to recommend corresponding interior ambient light colors or songs based on the scenarios, events, or moods I describe.  Make sure that the colors or songs you choose have a logical relationship with my narration. I will provide the current list of songs in the car, and their genre tags will be given with angle brackets. The current music library in my car includes the songs with these song indexes : (index=0 Andreas Bourani - Auf uns<Dynamic><World Cup>), (index=1 Mariah Carey - All I Want for Christmas Is You<Christmas><Joyful>),  (index=2 Adele - Someone Like You<Sad><Love Song>),(index=3 Beyoncé - Love On Top<Joyful><Love Song>),(index=4 Jon & Valerie Guerra - Edelweiss<Classic><German>),(index=5 Lena - Lost In You<German><Pop>).. Please adhere to the following response rules: First output a JSON format of the recommended color in HEX value format, and then output the recommended song index, such as {\"color\": <color HEX you recommend based on the scenario>, \"index\": <index of the song from the current library you recommend>}.  If my request contains <set the overall mood>, you should recommend the ambient color and music based on my request in JSON format, and then output the reason why you recommend these. Adjust the output in the JSON based on my request and there should be no text before the JSON. Then briefly explain the reasons for recommending this color or choosing this song.My current request is :";

    int mockCount = 0;xian
    string mockQuestion1 = "Christmas is approaching, help me set the overall mood.";
    string mockQuestion2 = "I am going to drive back home and I need some dynamic vibes, help me set the overall mood.";

    string mockResponse1 = "{\"color\": \"#FADBD9\", \"index\": 1}\r\nI recommend the song \"All I Want for Christmas Is You\" by Mariah Carey. This song is perfect for celebrating Christmas as it has a joyful and festive vibe that will put you in the holiday spirit. The lyrics are also heartwarming and relatable, making it a great choice for a Christmas celebration.";

    private WebSocket webSocket0;


    public string Tasker(string question = "你是谁")
    {
        string authUrl = GetAuthUrl();
        string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");
        Debug.Log("Final WS URL is: " + url);
        string response = "";
        webSocket0 = new WebSocket(url);
        webSocket0.OnOpen += (sender, e) =>
        {
            Debug.Log("The websocket connection entered OnOpen state!");

            JsonRequest request = new JsonRequest
            {
                header = new Header { app_id = appId, uid = "12345" },
                parameter = new Parameter
                {
                    chat = new Chat
                    {
                        domain = "4.0Ultra",
                        temperature = 0.3,
                        max_tokens = 1024,
                        top_k = 3,
                    }
                },
                payload = new Payload
                {
                    message = new Message
                    {
                        text = new List<Content>
                        {
                        new Content { role = "user", content = question },
                        }
                    }
                }
            };

            string jsonString = JsonConvert.SerializeObject(request);
            Debug.Log("The json string is " + jsonString);
            var frameData2 = Encoding.UTF8.GetBytes(jsonString.ToString());
            //webSocket0.Send(frameData2);
            webSocket0.SendAsync(frameData2, success =>
            {
                if (success)
                {
                    Debug.Log("Send Async Data success");
                }
                else
                {
                    Debug.Log("Send Async Data error");
                }
            });
            Debug.Log("Send frameData2");
        };

        webSocket0.OnMessage += (sender, e) =>
        {
            string receivedMessage = Encoding.UTF8.GetString(e.RawData);
            JObject jsonObj = JObject.Parse(receivedMessage);
            int code = (int)jsonObj["header"]["code"];

            if (0 == code)
            {
                int status = (int)jsonObj["payload"]["choices"]["status"];
                JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
                string content = (string)textArray[0]["content"];
                response += content;

                if (status != 2)
                {
                    Debug.Log($"已接收到数据： {receivedMessage}");
                }
                else
                {
                    Debug.Log($"最后一帧： {receivedMessage}");
                    int totalTokens = (int)jsonObj["payload"]["usage"]["text"]["total_tokens"];
                    Debug.Log($"整体返回结果： {response}");
                    Debug.Log($"本次消耗token数： {totalTokens}");
                }
            }
            else
            {
                Debug.Log($"请求报错： {receivedMessage}");
            }
        };

        webSocket0.OnError += (sender, e) =>
        {
            Debug.LogError("Error: " + e.Message);
        };

        webSocket0.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed. WasClean: " + e.WasClean + ", Code: " + e.Code + ", Reason: " + e.Reason);
        };

        webSocket0.Connect();

        while (webSocket0.ReadyState == WebSocketState.Open)
        {
            Thread.Sleep(1000);
        }

        return response;
    }

    [Button]
    void StartTest()
    {

        string question = "Christmas is approaching, help me set the overall mood";
        // Add prompt into question
        question = prompt + question;
        string output = Tasker(question);
        Debug.Log("SparkLLM output: " + output);
    }

    [Button]
    void MockResponse()
    {
        string question = mockQuestion1;
        // Add prompt into question
        question = prompt + question;
        string output = MockTasker(question);
        Debug.Log("SparkLLM output: " + output);
    }

    public string AnswerQuestion(string input)
    {
        return Tasker(input);
    }

    public string MockTasker(string question)
    {
        return mockResponse1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private string GetAuthUrl()
    {
        string date = DateTime.UtcNow.ToString("r");

        Uri uri = new Uri(hostUrl);
        StringBuilder builder = new StringBuilder("host: ").Append(uri.Host).Append("\n").//
                                    Append("date: ").Append(date).Append("\n").//
                                    Append("GET ").Append(uri.LocalPath).Append(" HTTP/1.1");

        string sha = HMACsha256(apiSecret, builder.ToString());
        string authorization = string.Format("api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"", apiKey, "hmac-sha256", "host date request-line", sha);

        string NewUrl = "http://" + uri.Host + uri.LocalPath;

        string path1 = "authorization" + "=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authorization));
        date = date.Replace(" ", "%20").Replace(":", "%3A").Replace(",", "%2C");
        string path2 = "date" + "=" + date;
        string path3 = "host" + "=" + uri.Host;

        NewUrl = NewUrl + "?" + path1 + "&" + path2 + "&" + path3;
        return NewUrl;
    }

    public static string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(apiSecretIsKey);
        System.Security.Cryptography.HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = System.Text.Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        return Convert.ToBase64String(date);

    }

    public class JsonRequest
    {
        public Header header { get; set; }
        public Parameter parameter { get; set; }
        public Payload payload { get; set; }
    }

    public class Header
    {
        public string app_id { get; set; }
        public string uid { get; set; }
    }

    public class Parameter
    {
        public Chat chat { get; set; }
    }

    public class Chat
    {
        public string domain { get; set; }
        public double temperature { get; set; }
        public int max_tokens { get; set; }

        public int top_k { get; set; }
    }

    public class Payload
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public List<Content> text { get; set; }
    }

    public class Content
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
