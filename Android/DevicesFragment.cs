using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevicesFragment : MonoBehaviour
{
    //private static readonly string UsbDevice = "android.hardware.usb.UsbDevice";
    //private static readonly string UsbManager = "android.hardware.usb.UsbManager";
    private static readonly string DevicesTestPack = "com.iav.misc.DevicesTest";

    //private AndroidJavaObject deviceFragment = new(DevicesFragmentPlugin);
    //AndroidJavaObject deviceFragment = new(DevicesTestPack);

    [Button]
    public void Refresh()
    {
        //if (deviceFragment == null) deviceFragment = new(DevicesTestPack);
        //deviceFragment.Call("refresh");
        DeviceTest.Refresh();
        Debug.Log("Refreshed");
        //AndroidJavaObject listitem = deviceFragment.Call<AndroidJavaObject>("getListItem");
        Listitem listitem = DeviceTest.GetListItem();
        Debug.Log(listitem.port);

        List<Listitem> list = DeviceTest.listitems;
        foreach (var item in list)
        {
            Debug.Log(item.port + ": " + item.GetDeviceInfo());
        }

        //List<AndroidJavaObject> list = JavaUtils.FromJavaList(deviceFragment.Call<AndroidJavaObject>("getListItems"));
        //foreach(AndroidJavaObject item in list)
        //{
        //    Debug.Log(item.Call<int>("port") + ": " + item.Call<string>("getDeviceInfo"));
        //}

        //int count = deviceFragment.Call<int>("getItemsCount");
        //for (int i = 0; i < count; i++)
        //{
        //    listitem = deviceFragment.Call<AndroidJavaObject>("getListItem", i);
        //    Debug.Log(listitem.Call<int>("port") + ": " + listitem.Call<string>("getDeviceInfo"));
        //}
    }
    public void Connect()
    {
        Connect(0);
    }
    public void Connect(int index)
    {
        //if (deviceFragment == null) return;
        //deviceFragment.Call("connect", index);
        DeviceTest.Connect(index);
        Debug.Log("Connected");
    }
    public void Disconnect()
    {
        //if (deviceFragment == null) return;
        //deviceFragment.Call("disconnect");
        DeviceTest.Disconnect();
        Debug.Log("Disconnected");
    }
    void OnApplicationQuit()
    {
        Disconnect();
    }
}
