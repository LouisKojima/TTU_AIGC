using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using Sirenix.OdinInspector;
using System.Text;
/// <summary>
/// 服务端 - Client
/// </summary>

//TODO: start,update,destroy

public class Client : MonoBehaviour
{
    public string serverIP = "127.0.0.1";

    [ToggleLeft, GUIColor("@valueChanged? Color.green : Color.red")]
    public bool valueChanged = false;

    public int serverPort = 1111;
    public float speed = 0;
    public float rotateSpeed = 0;
    public bool isSignalRight = false; //右转向灯
    public bool isSignalLeft = false;//左转向灯
    public bool isHighBeam = false;//远光灯
    public bool isLowerBeam = false;//近光灯

    //Used to check value chages
    private float _speed = 0;
    private float _rotateSpeed = 0;
    private bool _isSignalRight = false; 
    private bool _isSignalLeft = false;
    private bool _isHighBeam = false;
    private bool _isLowerBeam = false;

    private void CheckValueChange()
    {
        if (_speed != speed)
        {
            valueChanged = true;
            _speed = speed;
        }
        if (_rotateSpeed != rotateSpeed)
        {
            valueChanged = true;
            _rotateSpeed = rotateSpeed;
        }
        if (_isSignalRight != isSignalRight)
        {
            valueChanged = true;
            _isSignalRight = isSignalRight;
        }
        if (_isSignalLeft != isSignalLeft)
        {
            valueChanged = true;
            _isSignalLeft = isSignalLeft;
        }
        if (_isHighBeam != isHighBeam)
        {
            valueChanged = true;
            _isHighBeam = isHighBeam;
        }
        if (_isLowerBeam != isLowerBeam)
        {
            valueChanged = true;
            _isLowerBeam = isLowerBeam;
        }
    }

    private byte[] data = new byte[1024];

    private Socket socket;

    //void Start()
    //{      
    //    ConnectToServer();
    //}


    void Update()
    {
        CheckValueChange();
        if (socket != null && socket.Connected && socket.Available > 0) 
        {
            int bytesRead = socket.Receive(data);
            string message = Encoding.ASCII.GetString(data, 0, bytesRead);
            Debug.Log("Receive message from server: " + message);
        }
    }

    [Button]
    public bool ConnectToServer()
    {
        return ConnectToServer(serverIP, serverPort);
    }

    bool ConnectToServer(string ipAddress, int port)
    {
        try
        {
            Debug.Log("Client start.");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAddress, port);
            Debug.Log("Connected to server");
            //SendMessage("Hello Server!");
            //Debug.Log("Hello Server!");
            //SendMessage("55.00013200.1111");
            SendMsg();
            Debug.Log("Sent Speed Data");
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket Exception: " + e);
            return false;
        }
        catch(Exception e)
        {
            Debug.LogError("General Exception: " + e);
            return false;
        }
        return true;
    }
    [Button("Send Data")]
    public void SendMsg()
    {
        string message = DataMerge(speed, rotateSpeed, isHighBeam, isLowerBeam, isSignalRight, isSignalLeft);
        SendMsg(message);
    }
    [Button]
    public void SendMsg(string msg)
    {
        byte[] data = Encoding.ASCII.GetBytes(msg);
        socket.Send(data);
    }

    string DataMerge(float speed, float rotateSpeed, bool isSignalRight, bool isSignalLeft, bool isHighBeam, bool isLowerBeam)
    {
        string mergeData = FloatToString(speed) + FloatToString(rotateSpeed) + BoolToString(isSignalRight) + BoolToString(isSignalLeft) + BoolToString(isHighBeam) + BoolToString(isLowerBeam);
        return mergeData;
    }

    string FloatToString(float num)
    {
        string formattedFloat = num.ToString("0.0000");
        if (formattedFloat.Length < 6)
        {
            formattedFloat = formattedFloat.PadRight(6, '0');
        }
        else if (formattedFloat.Length > 6)
        {
            formattedFloat = formattedFloat.Substring(0, 6);
        }
        return formattedFloat;
    }

    string BoolToString(bool value)
    {
        if (value)
        {
            return "1";
        }
        else
        {
            return "0";
        }

    }


    public void Close()
    {
        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    private void OnDestroy()
    {
        socket.Close();
    }

    // 退出程序时自动关闭连接
    void OnApplicationQuit()
    {
        Close();
    }


}