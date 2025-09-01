using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JavaUtils
{
    public static List<AndroidJavaObject> FromJavaList(AndroidJavaObject javaList)
    {
        return FromJavaList<AndroidJavaObject>(javaList);
    }

    public static List<TItem> FromJavaList<TItem>(AndroidJavaObject javaList)
    {
        List<TItem> result = new();

        int count = javaList.Call<int>("size");
        for (int i = 0; i < count; i++)
        {
            result.Add(javaList.Call<TItem>("get",i));
        }

        return result;
    }
}
public class Listitem
{
    public readonly AndroidJavaObject self;
    public readonly AndroidJavaObject device;
    public readonly int port;
    public readonly AndroidJavaObject driver;

    public Listitem(AndroidJavaObject ajo)
    {
        self = ajo;
        device = self.Call<AndroidJavaObject>("device");
        port = self.Call<int>("port");
        driver = self.Call<AndroidJavaObject>("driver");
    }

    public string GetDeviceInfo()
    {
        return self.Call<string>("getDeviceInfo");
    }
}

public class DeviceTest
{
    private static readonly string DevicesTestPack = "com.iav.misc.DevicesTest";
    public static AndroidJavaObject deviceFragment = new(DevicesTestPack);
    private static int _baudRate = 115200;
    public static int baudRate
    {
        get
        {
            deviceFragment.Call("setBaudRate", _baudRate);
            return _baudRate;
        }
        set
        {
            deviceFragment.Call("setBaudRate", value);
            _baudRate = value;
        }
    }

    public static List<Listitem> listitems => JavaUtils.FromJavaList(deviceFragment.Call<AndroidJavaObject>("getListItems")).Select(x => new Listitem(x)).ToList();

    public static void Refresh()
    {
        deviceFragment.Call("refresh");
    }

    public static void Connect(int index)
    {
        deviceFragment.Call("setBaudRate", _baudRate);
        deviceFragment.Call("connect", index);
    }

    public static void Disconnect()
    {
        deviceFragment.Call("disconnect");
    }

    public static void SendData(byte[] data)
    {
        deviceFragment.Call("sendData", data);
    }

    public static void SendHexString(string toSend)
    {
        deviceFragment.Call("sendHexString", toSend);
    }

    public static Listitem GetListItem()
    {
        Listitem result = new(deviceFragment.Call<AndroidJavaObject>("getListItem"));
        return result;
    }

    public static List<Listitem> GetListItems()
    {
        //List<Listitem> list = JavaUtils.FromJavaList(deviceFragment.Call<AndroidJavaObject>("getListItems")).Select(x => new Listitem(x)).ToList();
        return listitems;
    }
}