using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeekplayDevice : MonoBehaviour
{
    protected string m_mac = null;

    #region 初始化
    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);
    }

    protected void InitBluetooth(string _deviceName)
    {
        Debug.Log("Bluetooth Initializing...");
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Bluetooth Initialized.");
            Scan(_deviceName);
        }, (err) =>
        {
            Debug.Log("Bluetooth Error: " + err);
            if ("Bluetooth LE Not Enabled" == err)
            {
                BluetoothLEHardwareInterface.FinishDeInitialize();
                BluetoothLEHardwareInterface.DeInitialize(ReInitBluetooth);
            }
        });
    }

    void ReInitBluetooth()
    {
        Invoke("InitBluetooth", 0.5f);      //  延时为了避免死循环
    }

    void Scan(string _targetName)
    {
        Debug.Log("Start scanning...");
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, (mac, name, rssi, adInfo) =>
        {
            Debug.Log("Get Scanned Result.");
            if (name.Equals(_targetName))
            {
                Debug.Log("Found: " + _targetName);
                BluetoothLEHardwareInterface.StopScan();
                Connect(mac);
            }
        });
    }

    void Connect(string _mac)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(_mac, (str) =>
        {
            Debug.Log("Connected.");
        }, (address, name) =>
        {
            //  等待 FFC0 服务开启后，就可以进行下一步
            if (name.Substring(4, 4).ToUpper() == "FFF0")
            {
                m_mac = _mac;
            }
        }, null, (str) =>
        {
            Debug.Log("Disconnected.");
        });
    }
    #endregion

    #region 订阅 notify 通道
    Dictionary<string, Action<byte[]>> subscribeHandlers = new Dictionary<string, Action<byte[]>>();
    void SubscribeHandler(string _channel, byte[] _data)
    {
        //  iOS 的通道号为大写，Android 为小写，因此统一为大写处理
        _channel = _channel.ToUpper().Substring(4, 4);
        if ((subscribeHandlers.ContainsKey(_channel)) && null != subscribeHandlers[_channel])
        {
            subscribeHandlers[_channel](_data);
        }
    }

    public IEnumerator Subscribe(string _service, string _channel, Action<byte[]> _handler)
    {
        Debug.Log("Start Subscribe.");
        subscribeHandlers.Add(_channel.ToUpper(), _handler);
        bool complete = false;
        BluetoothLEHardwareInterface.SubscribeCharacteristic(m_mac, _service, _channel, (str) =>
        {
            Debug.Log("Subscribe notification: " + str);
            complete = true;
        }, SubscribeHandler);
        yield return new WaitUntil(() => complete);
    }

    //  TODO: 是否需要修改？
    public IEnumerator UnSubscribe(string _service, string _channel)
    {
        bool complete = false;
        if (subscribeHandlers.ContainsKey(_channel.ToUpper()))
        {
            subscribeHandlers.Remove(_channel.ToUpper());
        }
        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(m_mac, _service, _channel, (str) => { complete = true; });
        yield return new WaitUntil(() => complete);
    }
    #endregion

    public string GetMAC()
    {
        return m_mac;
    }
}
