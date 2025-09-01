using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
/// <summary>
/// 可以供客户端和服务器一块使用
/// </summary>
public class TcpSocket
{
    //当前实例化的socket
    private Socket socket;
    //socket上的数据
    private byte[] data;
    //区分服务器  还是客户端
    private bool isServer;
    public TcpSocket(Socket socket, int dataLength, bool isServer)
    {
        this.socket = socket;

        data = new byte[dataLength];

        this.isServer = isServer;

    }

    #region 接受
    public void ClientReceive()
    {
        //data 数据缓存 0:指的是 接受位的偏移量   data.length指的是数据的长度   SocketFlags.None固定格式 
        //new AsyncCallback(ClientEndReceive)需要有返回值的回调函数,返回值是下面的方法
        //public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state);
        socket.BeginReceive(data, 0, data.Length, SocketFlags.None, new AsyncCallback(ClientEndReceive), null);
    }


    public void ClientEndReceive(IAsyncResult ar)
    {
        //数据的处理
        int receiveLength = socket.EndReceive(ar);    //将接受到的数据赋值给receiveLength
        //把接受完毕的字节数组转化为 string类型
        //string dataStr = System.Text.Encoding.Unicode.GetString(data, 0, receiveLength);
        string dataStr = System.Text.Encoding.ASCII.GetString(data, 0, receiveLength);

        if (isServer)    //判断是否接收到了
        {
            Debug.Log("服务器接收到了:" + dataStr);
            SilabData.instance.speed = float.Parse(dataStr.Substring(0, 6));
            //SilabData.instance.speed = float.Parse(dataStr.Substring(0, 6)) * 3.6f;
            SilabData.instance.rotateSpeed = float.Parse(dataStr.Substring(6, 6))/100;
            //SilabData.instance.rotateSpeed = float.Parse(dataStr.Substring(6, 6)) / 1000;
            SilabData.instance.isHighBeam = (int.Parse(dataStr.Substring(12, 1)) == 1);
            SilabData.instance.isLowerBeam = (int.Parse(dataStr.Substring(13, 1)) == 1);
            SilabData.instance.isSignalRight = (int.Parse(dataStr.Substring(14, 1)) == 1);
            SilabData.instance.isSignalLeft = (int.Parse(dataStr.Substring(15, 1)) == 1);
            //SilabData.instance.gear = float.Parse(dataStr.Substring(16,4));
            //SilabData.instance.p = int.Parse(dataStr.Substring(20,1));
            //SilabData.instance.isP = (int.Parse(dataStr.Substring(21, 1)) == 1);
            //float itemTTC;
            //try
            //{
            //    itemTTC = float.Parse(dataStr.Substring(22, 6));
            //}
            //catch
            //{
            //    itemTTC = 0f;
            //}
            //SilabData.instance.ttcTime = itemTTC;
            //SilabData.instance.ttc = int.Parse(dataStr.Substring(28, 1));


            // SilabData.instance.isTakeOver = (int.Parse(dataStr.Substring(21, 1)) == 1);
            //if (dataStr.Length == 23)
            //{
            //    SilabData.instance.isTakeOver = int.Parse(dataStr.Substring(21, 1)) == 1;
            //    SilabData.instance.isTakeOver1 = int.Parse(dataStr.Substring(22, 1)) == 1;
            //}
            // Debug.Log(SilabData.instance.gear);

            SilabData.instance.UpdateData();
        }
        else
        {
            Debug.Log("Client received:" + dataStr);

        }

    }
    #endregion


    #region 发送
    public void ClientSeed(byte[] data)    
    {
        socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(ClientSeedEnd), null);
    }

    private void ClientSeedEnd(IAsyncResult ar)
    {
        socket.EndSend(ar);
    }

    #endregion

    #region 连接
    public void ClientConnect(string ip, int port)
    {
        socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ClientEndConnect), null);

    }

    public void ClientEndConnect(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            Debug.Log("连接成功");
        }
        socket.EndConnect(ar);
    }
    #endregion

    #region 断开

    public bool ClientConnect()
    {
        return socket.Connected;
    }

    #endregion
}
