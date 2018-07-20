using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;


public enum DeviceName
{
    AR_Gun = 1, 
    ARcher = 2, 
    New_ARcher = 3
}

public class GeekplaySDK : MonoBehaviour
{
    //  设备类型与蓝牙名字的对应表
    static Dictionary<DeviceName, string[]> m_deviceNames = new Dictionary<DeviceName, string[]>
    {
        { DeviceName.AR_Gun, new string[]{ "GU-ARGUN" } },
        { DeviceName.ARcher, new string[]{ "GU-ARCHER" } },
        { DeviceName.New_ARcher, new string[]{ "R-ARCHER" } },
    };

    public DeviceName[] m_supportedDevices;
    GeekplayDevice m_device = null;
    
    public string m_userID = "10001";
    public string m_appID = "001";
    public string m_appName = "3rdPartyGame";
    public string m_appDescription = "This is just a 3rd party game.";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public delegate void RegisterCallback(GeekplayDevice _device);

    public void StartSDK(RegisterCallback _register)
    {
        StartCoroutine(CoStartSDK(_register));
    }

    IEnumerator CoStartSDK(RegisterCallback _register)
    {
        InitBluetooth(m_supportedDevices);
        yield return new WaitUntil(() => null != m_device);
        if (null != _register)
        {
            _register(m_device);
        }
    }

    void InitBluetooth(DeviceName[] _supportedDevices)
    {
        Debug.Log("Bluetooth Initializing...");
        BluetoothDeviceScript receiver = BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Bluetooth Initialized.");
            Scan(_supportedDevices);
        }, (err) =>
        {
            Debug.Log("Bluetooth Error: " + err);
            if ("Bluetooth LE Not Enabled" == err)
            {
                BluetoothLEHardwareInterface.FinishDeInitialize();
                BluetoothLEHardwareInterface.DeInitialize(() => { StartCoroutine(ReInitBluetooth(_supportedDevices)); });
            }
        });
        DontDestroyOnLoad(receiver.gameObject);
    }

    IEnumerator ReInitBluetooth(DeviceName[] _supportedDevices)
    {
        yield return new WaitForSeconds(0.5f);  //  延时为了避免死循环
        InitBluetooth(_supportedDevices);
    }

    void Scan(DeviceName[] _supportedDevices)
    {
        Debug.Log("Start scanning...");
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, (deviceID, name, rssi, adInfo) =>
        {
            for (int i = 0; i < _supportedDevices.Length; ++i)
            {
                for (int j = 0; j < m_deviceNames[_supportedDevices[i]].Length; ++j)
                {
                    string targetName = m_deviceNames[_supportedDevices[i]][j];
                    //  deviceID:   安卓上是 MAC 地址，iOS 上是某一串不明代码
                    //  adInfo:     广播数据，第 2-7 字节是 MAC 地址
                    if (name.Equals(targetName))
                    {
                        Debug.Log("Found: " + targetName);
                        BluetoothLEHardwareInterface.StopScan();
                        
                        m_device = GetDeviceScript(_supportedDevices[i]);
                        m_device.SetAppInfo(m_userID, m_appID, m_appName, m_appDescription);
                        m_device.m_deviceID = deviceID;
#if UNITY_IPHONE
                        m_device.m_MAC = GeekplayCommon.BytesToHexString(adInfo, "").Substring(4, 12);
#elif UNITY_ANDROID
                        m_device.m_MAC = deviceID.Replace(":", "");
#endif

                        return;
                    }
                }
            }

        });
    }

    GeekplayDevice GetDeviceScript(DeviceName _deviceName)
    {
        switch (_deviceName)
        {
            case DeviceName.AR_Gun:
                return gameObject.AddComponent<GeekplayARGun>();
            case DeviceName.ARcher:
                return gameObject.AddComponent<GeekplayARcher>();
            case DeviceName.New_ARcher:
                return gameObject.AddComponent<GeekplayNewARcher>();
            default:
                return gameObject.AddComponent<GeekplayDevice>();
        }
    }
}
