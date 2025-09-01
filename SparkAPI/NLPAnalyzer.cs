using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class NLPAnalyzer : MonoBehaviour
{
    //TODO: 1. Get from Baidu ASR's content, build it up to the desired prompt  获取 ASR 输入，和预设文本拼接，成为发送给 SparkAPI 的 prompt 
    // 2. Receive SparkAPI's response (multiple responses) 通过 websocket 连接 SparkAPI, 并获取返回信息 
    // 3. Compress multiple responses into one string    
    // 4. string contains two part: JSON and reason   解析返回的多条信息，并将其处理为两部分：JSON 和 REASON 
    // 5. parse the first JSON, and do actions if needed (1.COLOR 2.MUSIC 3.BOTH)  对于 JSON，我们需要解析并执行其中包含的操作(调整氛围色，选择歌曲并播放)
    // 6. Baidu TTS voice out the latter reason text    对于REASON，我们需要朗读这条文本


    // SparkAPI example reponse:
    // {"action": "BOTH","color": "#FFA500","index": 1}
    // 我推荐你选择红色作为氛围灯的颜色，因为红色通常被认为代表着喜庆、热情和活力。这与即将到来的圣诞节气氛相符。
    // 同时，我推荐你播放 "Mariah Carey - All I Want for Christmas Is You" 这首歌曲。这首歌曲是一首经典的圣诞歌曲，旋律优美，歌词温馨，能够营造出浓厚的节日氛围。

    // Double Mode: Command or Chat
    public enum DualMode { COMMAND, CHAT };
    const string commandPrompt = "Suppose you are one AI voice assistant inside a car. Your task is to recommend corresponding ambient light colors and songs based on the scenarios, events, or moods I describe. Make sure that the colors and songs you choose have a logical relationship with my narration. I will provide the current list of songs in the car, and their genre tags will be given with angle brackets. The current music library in my car includes the songs with these song indexes : (index=0 Eagles - The Hotel California<Rock>), (index=1 Mariah Carey - All I Want for Christmas Is You<Christmas>),(index=2 OneRepublic - Counting Stars<Pop>),(index=3 David Tao - Melody<Chinese><Classic>),(index=4 Adele - Someone Like You<Sad><Love Song>),(index=5 Andreas Bourani - Auf uns<Dynamic><World Cup>),(index=6 Beyoncé - Love On Top<Cheerful><Love Song>),(index=7 Jon & Valerie Guerra - Edelweiss<Classic>),(index=8 Lena - Lost In You<German><Pop>).. Please adhere to the following response rules: First output a JSON format of the recommended color in HEX value format, and then output the recommended song index. You should recommend the ambient color and music based on my request in JSON format. Your whole response example should be like: {\"color\": <color HEX you recommend based on the scenario>, \"index\": <index of the song from the current library you recommend>} .Now my current request is :";
    const string chatPrompt = "Suppose you are one AI voice assistant inside a car. Your task is to respond to user input with concise and brief response. My current request is:";
    [FoldoutGroup("ColorRefs")]
    public ColorCtrl colorCtrl;
    [FoldoutGroup("ColorRefs")]
    public ColorChangerSlider colorCtrl1;
    [FoldoutGroup("ColorRefs")]
    public ColorChangerSlider colorCtrl2;
    [FoldoutGroup("ColorRefs")]
    public PresetColorBtn customColorBtn;
    [FoldoutGroup("ColorRefs")]
    public PresetColorBtn presetColorBtn1;
    [FoldoutGroup("ColorRefs")]
    public PresetColorBtn presetColorBtn2;
    [FoldoutGroup("ColorRefs")]
    public PresetColorBtn presetColorBtn3;
    public SparkLLM sparkLLM;
    public MusicPlayer musicPlayer;
    string baiduASRResult1 = "Christmas is approaching, help me set the overall mood.";
    string baiduASRResult2 = "I am going to drive back home and I need some dynamic vibes, help me choose the overall mood.";
    string[] CommandModeKeyword = { "overall", "general" };
    string[] CommandModeVibeKeyword = { "change", "set", "choose", "ambiance", "ambience", "atomsphere", "song", "mood" };
    const string defaultReasonResponse = "I recommend this song and this color according to your request.";

    private string json = "", reason = "";


    // Build different question according to ASR is a Command or a Chat.
    private string BuildQuestion(string BaiduASRResult, DualMode mode)
    {
        if (mode == DualMode.COMMAND)
        {
            return commandPrompt + BaiduASRResult;
        }
        else
        {
            return chatPrompt + BaiduASRResult;
        }
    }

    [Button]
    // Mock Total process of the new NLP with Spark LLM
    public string MockTotalProcess(string input = "Christmas is approaching, help me set the overall mood.")
    {
        // Baidu's Chinese ASR Recognize

        string question = BuildQuestion(baiduASRResult1, DualMode.COMMAND);
        // Get response from Mock Spark API
        string response = sparkLLM.MockTasker(question);
        Debug.Log("The response is: " + response);

        //TODO: Parse the response, extract JSON & REASON
        ExtractJson(response, out json, out reason);
        ActionParser(json, reason);
        return reason;
    }

    [Button]
    // Total process of the new NLP with Spark LLM
    public string TotalProcess(string input = "Help me set the overall vibe which is blue and sad.")
    {
        // Baidu's Chinese ASR Recognize
        DualMode answerMode = DetermineMode(input);
        string question = BuildQuestion(input, answerMode);
        Debug.Log("The whole question is: " + question);
        // Get response from Mock Spark API
        string response = sparkLLM.Tasker(question);
        Debug.Log("The response is: " + response);

        //Parse the response, extract JSON & REASON
        //string json, reason;
        // if it's a Command Mode, extract the JSON
        // if it's a Chat Mode, just output the reason in plain text
        if (answerMode == DualMode.COMMAND)
        {
            ExtractJson(response, out json, out reason);
        }
        else
        {
            reason = response;
        }

        return reason;
    }

    public void DoAction()
    {
        ActionParser(json, reason);
    }


    /////////////////////////////////// Analyzing JSON & Do Action Related //////////////////////////////

    private bool ActionParser(string json, string reason)
    {
        // First let json get parsed
        JsonCommand jsonCommand = JsonUtility.FromJson<JsonCommand>(json);
        if (jsonCommand.color != "")
        {
            AmbientColorChange(jsonCommand.color);
        }
        if (jsonCommand.index != -1)
        {
            PlayMusic(jsonCommand.index);
        }
        return true;
    }


    private string ResponseParser(string SparkResponse)
    {
        //TODO: First parse the JSON, then voice out the reason text
        //Question: Can it be realized that when SparkResponse still not ends, and JSON ends. And we handle the JSON first?

        // if has color in JSON, change color
        // if has index, play the music
        // Notice: It's better to play the music after TTS voice out



        return null;
    }

    // Extract JSON and Reason from Spark Response
    private void ExtractJson(string input, out string json, out string reason)
    {
        int bracketCount = 0;
        int jsonEndIndex = 0;

        // 遍历输入字符串，找到完整的 JSON 对象的结尾
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '{')
            {
                bracketCount++;
            }
            else if (input[i] == '}')
            {
                bracketCount--;
                if (bracketCount == 0)
                {
                    jsonEndIndex = i + 1;
                    break;
                }
            }
        }

        // 截取 JSON 和其他内容
        json = input.Substring(0, jsonEndIndex);

        // Get music name from index
        JsonCommand jsonCommand = JsonUtility.FromJson<JsonCommand>(json);
        string musicName = musicPlayer.GetTrackNameByIndex(jsonCommand.index);
        reason = "I recommend this color and music " + musicName + " according to your request";

        // 打印结果
        Debug.Log("JSON Segment: " + json);
    }

    private class JsonCommand
    {
        public string color;
        public int index;
    }

    /////////////////////////////////// Color Change Action Related //////////////////////////////


    public bool TryConvertHexToRGB(string hex, out byte r, out byte g, out byte b)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1); // 移除开头的 '#'
        }

        if (hex.Length == 6)
        {
            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return true;
        }
        else
        {
            r = g = b = 0;
            return false;
        }
    }

    [Button]
    public void AmbientColorChange(string hex)
    {
        byte r, g, b;
        TryConvertHexToRGB(hex, out r, out g, out b);
        Color colorDisplay = new Color32(r, g, b, 255);
        float hue, saturation, brightness;
        Color.RGBToHSV(colorDisplay, out hue, out saturation, out brightness);
        colorCtrl1.SetColor(hue, saturation, brightness);
        colorCtrl2.SetColor(hue, saturation, brightness);

        //colorCtrl.UsePreset(customColorBtn);
        presetColorBtn1.GetComponent<Toggle>().isOn = false;
        presetColorBtn2.GetComponent<Toggle>().isOn = false;
        presetColorBtn3.GetComponent<Toggle>().isOn = false;
        customColorBtn.GetComponent<Toggle>().isOn = true;
        //customColorBtn.AcceptCustom();

        Debug.Log("Already Changed color");
    }

    /////////////////////////////////// Music Player & Voice Out Action Related //////////////////////////////

    private void ReasonVoiceOut(string Reason)
    {
        //TODO: When SparkAPI finishes output, TTS voice out the reason text
        Debug.Log("Reason Voiced Out");
        return;
    }

    private void PlayMusic(int index)
    {
        //Play the music according to index
        musicPlayer.PlayAtIndex(index);
        Debug.Log("Play the music of index: " + index);
    }

    /////////////////////////////////////// Add Chat Mode to respond common request //////////////////////////////////

    /// TODO: 1. First identify if user's request contains 'help me' , 'mood'
    /// If contains 'help me' or 'mood', enter Command Mode
    /// If do not contain, enter Chat Mode
    public string MockDualModeProcess()
    {
        // Baidu's Chinese ASR Recognize
        // build question with two mode : Chat or Command
        DualMode currentMode = DetermineMode(baiduASRResult1);

        string question = BuildQuestion(baiduASRResult1, DualMode.COMMAND);

        if (currentMode == DualMode.COMMAND)
        {
            // Get response from Mock Spark API
            string response = sparkLLM.MockTasker(question);
            Debug.Log("The response is: " + response);

            // Parse the response, extract JSON & REASON
            ExtractJson(response, out json, out reason);
            ActionParser(json, reason);
            return reason;
        }
        else
        {
            string response = "";
            Debug.Log("Chat Mode response is: " + response);
            ReasonVoiceOut(response);
            return null;
        }


    }

    // Analyze the asr result text, to classify if it's a Chat Mode or a Command Mode

    public bool CheckCommandKeywordInString(string input)
    {
        bool ifContainsVibeKey = false;

        foreach (var keyword in CommandModeVibeKeyword)
        {
            if (input.Contains(keyword, System.StringComparison.OrdinalIgnoreCase))
            {
                ifContainsVibeKey = true;
                break;
            }
        }
        return ifContainsVibeKey;
    }

    public DualMode DetermineMode(string input)
    {
        if (CheckCommandKeywordInString(input))
        {
            return DualMode.COMMAND;
        }
        else
        {
            return DualMode.CHAT;
        }
    }

}
