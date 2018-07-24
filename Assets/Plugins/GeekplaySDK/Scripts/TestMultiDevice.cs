using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMultiDevice : MonoBehaviour
{
    List<GeekplayDevice> deviceList = new List<GeekplayDevice>();


	void Start ()
    {
        Debug.Log("Bluetooth Initializing...");
        BluetoothDeviceScript receiver = BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Bluetooth Initialized.");
            Debug.Log("Start scanning...");
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, (deviceID, name, rssi, adInfo) =>
            {
                //  deviceID:   安卓上是 MAC 地址，iOS 上是某一串不明代码
                //  adInfo:     广播数据，第 2-7 字节是 MAC 地址
                if (("R-ARCHER" == name) || ("GU-ARCHER" == name))
                {
                    Debug.Log("Found: " + name);
                    //BluetoothLEHardwareInterface.StopScan();

                    GeekplayDevice device = GetDeviceScript(name);
                    device.SetAppInfo("", "", "", "");
                    device.m_deviceID = deviceID;
                    device.m_MAC = deviceID.Replace(":", "");

                    deviceList.Add(device);
                    if (typeof(GeekplayHunter) == device.GetType())
                    {
                        ((GeekplayHunter)device).Initialize(BowDraw, BowShoot);
                    }
                    else if (typeof(GeekplayDragonbone) == device.GetType())
                    {
                        ((GeekplayDragonbone)device).Initialize(BowDraw, BowShoot);
                    }
                }
            });
        }, null);
    }

    GeekplayDevice GetDeviceScript(string _deviceName)
    {
        switch (_deviceName)
        {
            case "GU-ARCHER":
                return gameObject.AddComponent<GeekplayHunter>();
            case "R-ARCHER":
                return gameObject.AddComponent<GeekplayDragonbone>();
            default:
                return gameObject.AddComponent<GeekplayDevice>();
        }
    }

    void BowDraw()
    {
        Debug.Log("Bow Draw: " + Time.time);
    }

    void BowShoot()
    {
        Debug.Log("Bow Shoot: " + Time.time);
    }
}
