using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Text;
using static PresetColors;
using static ColorChangerSlider;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
//#if UNITY_ANDROID
//#else
using System.IO.Ports;
using System.Linq;
//#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PortControl : MonoBehaviour

{
    public Toggle lightSwitch;
    public Slider brightness1;
    public Slider brightness2;
    public ColorCtrl colorCtrl;

    public bool halfBandMode = false;

    #region 定义串口属性
    //#if UNITY_ANDROID
    //#else
    public TMP_InputField portNameField;
    public TMP_Dropdown portNameSelection;
    //public GUIText Test;
    //定义基本信息
#if UNITY_EDITOR
    private void EditName()
    {
        if (!portNameField) return;
        portNameField.text = portName;
    }
    [OnValueChanged(nameof(EditName))]
#endif
    [DelayedProperty]
    public string portName = "COM7";//串口名
    public int baudRate = 115200; // 波特率
    public Parity parity = Parity.None; //校验位   
    public int dateBits = 8; // 数据位
    public StopBits stopBits = StopBits.One;
    static SerialPort sp = null;
    Thread dataReceiveThread;
    //#endif
    //发送的信息
    string message = "";
    public List<byte> listReceive = new List<byte>();
    char[] strchar = new char[100];
    public const string head = "43 48 49 4E 41 00 00 01 68";
    public string str;

    //time delay
    public float startTime;
    public float curTime;
    public float interval = 2f;  // 计时器的时间间隔

    //呼吸时间
    public int breatheIndexCount = 0;
    //public int normalDistributionCount = 90;
    [Min(0.1f)]
    public float distributionWidth = 10;
    [Range(0, 120)]
    public int distributionOffset = 60;
    [Range(0, 1)]
    public float distributionMin = 0.1f;
    public float distributionSpeed = 2f;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //#if UNITY_ANDROID
        //#else
        UpdateDeviceList();
        dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
        dataReceiveThread.Start();

        if (portNameSelection && portNameField)
            portNameSelection.onValueChanged.AddListener(
                x => portNameField.text = portNameSelection.options[x].text);

        startTime = Time.frameCount;
        //#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (lightSwitch == null || lightSwitch.isOn)
        {
#if UNITY_ANDROID
#else
            //Debug.Log(ColorChangerSlider.chosenCol);
            if (SpeechRecognition.m_PhraseRecognizer == null)
#endif
            {

                /*            string str = TransferRGBToHex(chosenCol);
                            WriteData(StringToBytes(str));*/


                if (breatheIndexCount >= index_wave.Length) { breatheIndexCount = 0; }
                else
                {
                    curTime = Time.frameCount;
                    if (curTime - startTime > interval)
                    {
                        str = TransferRGBToHex(colorCtrl.mainColor, colorCtrl.secondaryColor);
                        //str = TransferRGBToHexWithBreathing(colorCtrl.mainColor);
                        //str = TransferRGBToHexWithNormalDistribution(colorCtrl.mainColor);
                        //Debug.Log(str);
#if UNITY_ANDROID
                        SendRawString(str);
#else
                        WriteData(StringToBytes(str));
#endif
                        breatheIndexCount++;
                        //Debug.Log(breatheIndexCount);
                        startTime = curTime;
                    }

                }

                /*            if (normalDistributionCount >= 210) { normalDistributionCount = 90; }
                            else
                            {
                                curTime = Time.frameCount;
                                if (curTime - startTime > interval)
                                {
                                    string str = TransferRGBToHexWithNormalDistribution(chosenCol);
                                    Debug.Log(str);
                                    WriteData(StringToBytes(str));
                                    normalDistributionCount++;
                                    Debug.Log(normalDistributionCount);
                                    startTime = curTime;
                                }

                            }*/



            }
#if UNITY_ANDROID
#else
            else
            {
                SpeechRecognition.m_PhraseRecognizer.Dispose();
            }
#endif
        }
        else
        {
            str = TransferRGBToHex(Color.black);
#if UNITY_ANDROID
            SendRawString(str);
#else
            WriteData(StringToBytes(str));
#endif
        }


        //Debug.Log(str);

    }

    //get numbers from Color(r,g,b,a) to a string "g.IntToHex, b.IntToHex, r.IntToHex" grb
    public string TransferRGBToHex(Color input)
    {
        //Adapt Brightness
        float h, s, v;
        Color.RGBToHSV(input, out h, out s, out v);
        Color color = Color.HSVToRGB(h, s, brightness1.normalizedValue * v);

        StringBuilder sb = new StringBuilder(head); // default command
        float red = color[0] * 255; float green = color[1] * 255; float blue = color[2] * 255;
        for (int i = 0; i < 120; i++)
        {
            sb.Append(" ");
            sb.Append(((int)green).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)red).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)blue).ToString("x2"));

        }
        string str = sb + "";
        return str;

    }
    public string TransferRGBToHex(Color input1, Color input2)
    {
        //Adapt Brightness
        float h1, s1, v1, h2, s2, v2;
        Color.RGBToHSV(input1, out h1, out s1, out v1);
        Color color = Color.HSVToRGB(h1, s1, brightness1.normalizedValue * v1);
        Color.RGBToHSV(input2, out h2, out s2, out v2);
        Color color2 = Color.HSVToRGB(h2, s2, brightness2.normalizedValue * v2);

        StringBuilder sb = new StringBuilder(head); // default command
        float red = color[0] * 255; float green = color[1] * 255; float blue = color[2] * 255;
        float red2 = color2[0] * 255; float green2 = color2[1] * 255; float blue2 = color2[2] * 255;
        for (int i = 0; i < 60; i++)
        {
            sb.Append(" ");
            sb.Append(((int)green).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)red).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)blue).ToString("x2"));
        }
        for (int i = 0; i < 60; i++)
        {
            sb.Append(" ");
            sb.Append(((int)green2).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)red2).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)blue2).ToString("x2"));
        }
        string str = sb + "";
        return str;

    }
    public string TransferRGBToHexWithBreathing(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        v = Math.Max(brightness1.normalizedValue * (float)(index_wave[breatheIndexCount]) / 255, 0.01f);
        Color color1 = Color.HSVToRGB(h, s, v);
        StringBuilder sb = new StringBuilder(head); // default command
        float red = color1[0] * 255; float green = color1[1] * 255; float blue = color1[2] * 255;
        for (int i = 0; i < 120; i++)
        {
            sb.Append(" ");
            sb.Append(((int)green).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)red).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)blue).ToString("x2"));

        }
        string str = sb + "";
        return str;

    }

    public string TransferRGBToHexWithNormalDistribution(Color color)
    {

        StringBuilder sb = new StringBuilder(head); // default command
        for (int i = 0; i < 120; i++)
        {
            Color.RGBToHSV(color, out float h, out float s, out _);

            float sinX = (float)breatheIndexCount / index_wave.Length * (float)Math.PI * 2 * distributionSpeed;
            int offset = Mathf.RoundToInt((MathF.Sin(sinX) + 1) / 2 * 120);
            float v = Math.Max(Distribution(i - offset, distributionWidth), 0.01f);
            v = Mathf.Lerp(distributionMin, 1, v);
            v = Math.Max(brightness1.normalizedValue * v, 0.01f);
            Color color1 = Color.HSVToRGB(h, s, v);
            float red = color1[0] * 255; float green = color1[1] * 255; float blue = color1[2] * 255;
            sb.Append(" ");
            sb.Append(((int)green).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)red).ToString("x2"));
            sb.Append(" ");
            sb.Append(((int)blue).ToString("x2"));
        }
        string str = sb + "";
        return str;
    }

    //[Button]
    //public List<float> TestDistribution(float w = 1)
    //{
    //    List<float> result = new();
    //    for (int x = -5; x <= 5; x++)
    //    {
    //        result.Add(Distribution(x, w));
    //    }
    //    return result;
    //}
    /*    public string TransferRGBToHexWithMoving(Color color) 
        { 

        }*/
    #region 创建串口，并打开串口

    private int portIndex;
    public void SelectPortName(int index)
    {
        if (index >= names.Length) return;
        if (portNameField) portNameField.text = names[index];
        portName = names[index];
        portIndex = index;
    }
    public string[] names;
    public void UpdateDeviceList()
    {
#if UNITY_ANDROID
        DeviceTest.Refresh();
        names = DeviceTest.GetListItems().Select(x => x.GetDeviceInfo()).ToArray();
#else
        try
        {
            names = SerialPort.GetPortNames();
        }
        catch (Exception e)
        {
            names = new string[1];
            names[0] = e.Message;
        }
#endif

        if (!portNameSelection) return;

        portNameSelection.options = names.Select(x => new TMP_Dropdown.OptionData(x)).ToList();
        if (portNameSelection.options.Count == 0) portNameSelection.options.Add(new("No Device"));
    }

    public void OpenPort()
    {
        UpdateDeviceList();
        if (portNameField)
            portName = portNameField.text;

        try
        {
            //创建串口
#if UNITY_ANDROID
            DeviceTest.Connect(portIndex);
#else
            sp = new SerialPort(portName, baudRate, parity, dateBits, stopBits);
            sp.ReadTimeout = 400;
            sp.Open();
#endif
            Debug.Log("打开端口成功");
            portNameField.targetGraphic.color = Color.green;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            portNameField.targetGraphic.color = Color.red;
        }
    }
    #endregion

    #region 程序退出时关闭串口
    void OnApplicationQuit()
    {
        ClosePort();
    }

    public void ClosePort()
    {
        try
        {
#if UNITY_ANDROID
            SendRawString(ALLOFF);
            DeviceTest.Disconnect();
#else
            if (sp != null && sp.IsOpen)
            {
                WriteData(StringToBytes(ALLOFF));
                sp.Close();
                if (portNameField && portNameField.targetGraphic)
                {
                    portNameField.targetGraphic.color = Color.white;
                }
                Debug.Log("串口已关闭");
            }
            else
            {
                Debug.Log("没有已连接的串口，无需关闭。");
            }
#endif
            if (dataReceiveThread != null && dataReceiveThread.IsAlive)
            {
                dataReceiveThread.Abort();
                Debug.Log("数据接收线程已停止");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("关闭端口时发生错误: " + ex.Message);
        }
    }
    #endregion

    public void RefreshPort()
    {
        ClosePort();
        OpenPort();
    }


    /// <Summary>
    /// 打印接收到的信息
    /// </Summary>>
    /// 
    void PrintData()
    {
        for (int i = 0; i < listReceive.Count; i++)
        {
            strchar[i] = (char)(listReceive[i]);
            str = new string(strchar);
        }
        Debug.Log(str);
    }
    void PrintData(string str)
    {
        Debug.Log(str);
    }

    #region 接收数据
    void DataReceiveFunction()
    {
#if UNITY_ANDROID
#else
        #region 按字节数组发送处理信息
        byte[] buffer = new byte[1024];
        int bytes = 0;
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    bytes = sp.Read(buffer, 0, buffer.Length);//接收字节
                    if (bytes == 0)
                    {
                        Debug.Log("Input is null");
                        continue;
                        // no input
                    }
                    else
                    {
                        string strbytes = Encoding.Default.GetString(buffer);
                        //Debug.Log(strbytes);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(ThreadAbortException))
                    {
                    }
                }
            }
            Thread.Sleep(10);

        }
        #endregion
#endif
    }
    #endregion

    #region 发送数据
    /*    void DataSendFunction() 
        {
            *//*        while (true) 
                    {   

                    }*//*
            Debug.Log(ColorChangerSlider.chosenCol);
            //Color[] colors = new Color[ColorChangerSlider.textureHeight];

        }*/


    public static void WriteData(byte[] dataStr)
    {
#if UNITY_ANDROID
        DeviceTest.SendData(dataStr);
        //Debug.Log("发送成功");
#else
        if (sp != null && sp.IsOpen)
        {
            sp.Write(dataStr, 0, dataStr.Length);
            //Debug.Log("发送成功");
            //Debug.Log(dataStr);
            //Debug.Log(dataStr.Length);
        }
#endif
    }

    public static byte[] StringToBytes(string str)
    {
        str = str.Replace(" ", "");
        if ((str.Length % 2) != 0)
            str += " ";
        byte[] bText = new byte[str.Length / 2];
        for (int i = 0; i < bText.Length; i++)
        {
            bText[i] = Convert.ToByte(Convert.ToInt32(str.Substring(i * 2, 2), 16));
        }
        return bText;
    }

    /*    void OnGUI()
        {
            message = GUILayout.TextField(message);
            if (GUILayout.Button("Send Input"))
            {
                WriteData(StringToBytes(message));
            }
            if (GUILayout.Button("All Red Test"))
            {
                WriteData(StringToBytes(PresetColors.ALLRED));
            }
            if (GUILayout.Button("All Green Test"))
            {
                WriteData(StringToBytes(PresetColors.ALLGREEN));
            }
            if (GUILayout.Button("All Blue Test"))
            {
                WriteData(StringToBytes(PresetColors.ALLBLUE));
            }
            if (GUILayout.Button("All Purple Test"))
            {
                WriteData(StringToBytes(PresetColors.ALLPURPLE));
            }
            if (GUILayout.Button("All Yellow Test"))
            {
                WriteData(StringToBytes(PresetColors.ALLYELLOW));
            }

            //string test = "123";
            if (GUILayout.Button("Turn Off")) 
            {
                WriteData(StringToBytes(PresetColors.ALLOFF));

            }

        }*/
    #endregion
    #region Test
    public TMP_InputField singleColHex;
    public TMP_InputField numberOfLed;
    public TMP_InputField toSend;

    public void GenerateToSend()
    {
        string tosend = "4348494E41 0000 ";
        int nLed = Convert.ToInt32(numberOfLed.text);
        string num = (nLed * 3).ToString("x4");
        StringBuilder sb = new StringBuilder(tosend + num);

        for (int i = 0; i < nLed; i++)
        {
            sb.Append(" ");
            sb.Append(singleColHex.text);
        }

        toSend.text = sb + "";
    }

    public void SendString()
    {
        WriteData(StringToBytes(toSend.text));
    }

    public void SendRawString()
    {
#if UNITY_ANDROID
        DeviceTest.SendHexString(toSend.text);
#endif
    }

    public void SendRawString(string s)
    {
#if UNITY_ANDROID
        DeviceTest.SendHexString(s);
#endif
    }

    #endregion
    #region Preview
#if UNITY_EDITOR

    private List<Color> lights = new();
    private string[] s;

    [OnInspectorGUI]
    private void OnInspectorUpdate()
    {
        ReadColors();
        DrawLightsC();
    }

    private void ReadColors()
    {
        s = str.Remove(0, PortControl.head.Length).ToUpper().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < s.Length / 3; i++)
        {
            int g = Convert.ToInt32(s[3 * i], 16);
            int r = Convert.ToInt32(s[3 * i + 1], 16);
            int b = Convert.ToInt32(s[3 * i + 2], 16);
            Color result = new Color(r / 255f, g / 255f, b / 255f);
            if (lights.Count > i)
            {
                lights[i] = result;
            }
            else
            {
                lights.Add(result);
            }
        }

    }

    private int width = 32;
    private int height = 32;

    //private void DrawLights()
    //{
    //    Rect rect = EditorGUILayout.BeginHorizontal();
    //    int rowCount = Mathf.CeilToInt(EditorGUILayout.GetControlRect().width) / width + 1;
    //    for (int i = 0; i < lights.Count; i++)
    //    {
    //        int x = i % rowCount;
    //        int y = i / rowCount;
    //        EditorGUI.DrawRect(new(x * width + rect.x, y * height + rect.y, width, height), lights[i]);
    //    }
    //    EditorGUILayout.EndHorizontal();
    //}

    //private void DrawLightsB()
    //{
    //    Rect rect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
    //    int columnCount = Mathf.CeilToInt(EditorGUILayout.GetControlRect().width) / width + 1;
    //    int rowCount = lights.Count / columnCount + 1;
    //    //EditorGUI.DrawRect(rect, Color.black);
    //    for (int i = 0; i < lights.Count; i++)
    //    {
    //        int x = i % columnCount * width + (int)rect.x;
    //        int y = i / columnCount * height + (int)rect.y;
    //        EditorGUI.DrawRect(new(x, y, width, height), lights[i]);

    //        Color c = new();
    //        Color.RGBToHSV(c, out _, out _, out float v);
    //        c.r = MathF.Round(lights[i].r * 255) / 255;
    //        c.g = MathF.Round(lights[i].g * 255) / 255;
    //        c.b = MathF.Round(lights[i].b * 255) / 255;
    //        Color.RGBToHSV(c, out float h, out float s, out _);
    //        c = Color.HSVToRGB(h, s, 1);
    //        float sqrV = MathF.Sqrt(v);
    //        int iwidth = Mathf.RoundToInt(Mathf.Lerp(2, width, sqrV) / 2) * 2;
    //        int iheight = Mathf.RoundToInt(Mathf.Lerp(2, height, sqrV) / 2) * 2;
    //        Rect r = new(
    //            x + iwidth / 2,
    //            y + iheight / 2,
    //            iwidth,
    //            iheight);
    //        EditorGUI.DrawRect(r, c);
    //    }
    //    Debug.Log(columnCount + " " + rowCount);
    //    EditorGUILayout.LabelField("", GUILayout.Height(Mathf.Max(3, rowCount) * height));
    //    EditorGUILayout.EndVertical();
    //}

    private void DrawLightsC()
    {
        GUILayout.Label(
            new GUIContent(GenerateLightsTexture()),
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));
    }

    [Range(10, 30)]
    public int columnCount = 21;

    [Min(1), MaxValue(255)]
    [InlineButton(nameof(ResetBrightnessRes), "↺")]
    [InlineButton(nameof(RaiseBrightnessRes), ">")]
    [InlineButton(nameof(LowerBrightnessRes), "<")]
    public int brightnessRes = 127;

    private void RaiseBrightnessRes()
    { brightnessRes = brightnessRes < 255 ? brightnessRes + 1 : 255; }
    private void LowerBrightnessRes()
    { brightnessRes = brightnessRes > 1 ? brightnessRes - 1 : 1; }
    private void ResetBrightnessRes() { brightnessRes = 127; }

    private Texture2D GenerateLightsTexture()
    {
        int rowCount = lights.Count / columnCount + (lights.Count % columnCount == 0 ? 0 : 1);
        var result = new Texture2D(columnCount * width, rowCount * height);
        //result.wrapMode = TextureWrapMode.Clamp;
        //result.hideFlags = HideFlags.DontSave;

        for (int i = 0; i < lights.Count; i++)
        {
            int x = i % columnCount * width;
            int y = i / columnCount * height;
            //Debug.Log("i: " + i + "x: " + x + "y: " + y);

            Color c = new();
            Color.RGBToHSV(lights[i], out _, out _, out float v);
            c.r = MathF.Round(lights[i].r * brightnessRes) / brightnessRes;
            c.g = MathF.Round(lights[i].g * brightnessRes) / brightnessRes;
            c.b = MathF.Round(lights[i].b * brightnessRes) / brightnessRes;
            Color.RGBToHSV(c, out float h, out float s, out _);
            c = Color.HSVToRGB(h, s, 1);
            float sqrV = MathF.Sqrt(v);
            int iwidth = Mathf.RoundToInt(Mathf.Lerp(2f, width, sqrV) / 2) * 2;
            int iheight = Mathf.RoundToInt(Mathf.Lerp(2f, height, sqrV) / 2) * 2;

            for (int ox = x; ox < x + width; ox++)
            {
                for (int oy = y; oy < y + height; oy++)
                {
                    bool condition =
                        (ox > x + (width - iwidth) / 2 && ox < x + (width + iwidth) / 2) &&
                        (oy > y + (height - iheight) / 2 && oy < y + (height + iheight) / 2);

                    result.SetPixel(ox, result.height - oy, condition ? c : lights[i]);
                }
            }
        }

        result.Apply();
        return result;
    }
#endif
    #endregion
}
