using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using Sirenix.OdinInspector;
/// <summary>
/// 服务器
/// </summary>
public class Server : MonoBehaviour {
    private static Server instance;

    public static Server Instance
    {
        get {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private Socket server;

    //所有连接的客户端
    public List<TcpSocket> clients;


    private bool isLoopAccept = true;

    public int receivePort = 1111;
	// Use this for initialization
	void Start ()
    {
        //  服务器socket        协议族   
        server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        //绑定端口号 端口号默认 1111
        while (receivePort <= 9999)
        {
            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, receivePort));
                Debug.Log("Using Port " + receivePort + ". ");
                break;
            }
            catch(SocketException e)
            {
                Debug.LogWarning("Cannot use Port " + receivePort + ". " + e.Message);
                receivePort++;
            }
        }
        if (receivePort > 9999) return;

        //可以监听的客户端数目
        server.Listen(100);
        
        //开辟一个线程   处理客户端连接请求
        Thread listenThread = new Thread(ReceiveClient);
        //开启线程
        listenThread.Start();
        //后台运行
        listenThread.IsBackground = true;
        Debug.Log("开启线程");

        clients = new List<TcpSocket>();
    }

    /// <summary>
    /// 接受客户端连接的请求
    /// </summary>
    private void ReceiveClient()
    {
        while (isLoopAccept)
        {
            //开始接受客户端连接请求
            server.BeginAccept(AcceptClient,null);
         
            //每隔1s检测 有没有连接我
            Thread.Sleep(1000);
        }
        
    }

    [Button]
    public void SendMsg(string toSend = "233333LOL")
    {
        //if (!Application.isPlaying) return;
        clients.ForEach(ts => 
        {
            ts.ClientSeed(System.Text.Encoding.ASCII.GetBytes(toSend));
        });
    }
    private void OnApplicationQuit()
    {
        isLoopAccept = false;
        if (server != null && server.Connected)
        {
            server.Close();
        }
    }

    /// <summary>
    /// 客户端连接成功之后得回调
    /// </summary>
    /// <param name="ar"></param>
    private void AcceptClient(IAsyncResult ar)
    {
        Socket client = server.EndAccept(ar);

        TcpSocket clientSocket = new TcpSocket(client,1024,true);
        clients.Add(clientSocket);
        SilabData.instance.isConnectTcp = true;
        Debug.Log("连接成功");
        SendMsg("Unity Connected");
    }
    private void Update()
    {
        if (clients!= null&& clients.Count>0 )
        {
           
            if (clients[0].ClientConnect())
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    //监听每个服务器(连接的)发送的信息
                    clients[i].ClientReceive();
                    try
                    {
                        //clients[i].ClientSeed(System.Text.Encoding.Unicode.GetBytes("服务器回复:检测"));

                    }
                    catch (Exception e)
                    {
                        SilabData.instance.isConnectTcp = false;
                        Debug.Log("连接断开");
                        Debug.Log(e.Message);
                    }
                    //clients[i].ClientEndReceive();
                }
            }
            else
            {
                clients.Remove(clients[0]);
                
                SilabData.instance.isConnectTcp = false;
                SilabData.instance.Init();
               // GetComponent<MainUI>().SetRotationPointer();
                Debug.Log("连接断开");
            }
            
        } 
    }
}
