using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogScreen : MonoBehaviour
{
    [Range(1, 15)]
    public uint qsize = 10;  // number of messages to keep
    [Range(8, 108)]
    public int fontSize = 48;
    Queue myLogQueue = new Queue();

    public void setFontSize(float i)
    {
        fontSize = Mathf.RoundToInt(i);
    }

    void Start()
    {
        Debug.Log("Started up logging.");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        myLogQueue.Clear();
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUIStyle style = new();
        style.fontSize = fontSize;
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()), style);
        GUILayout.EndArea();
    }
}
