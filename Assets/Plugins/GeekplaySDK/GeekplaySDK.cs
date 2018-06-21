using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AR_Gun
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

public class GeekplaySDK : MonoBehaviour
{
    AR_Gun m_arGun = new AR_Gun();
    Action ShootHandler;
    
    public string m_hardwareName = null;
    public string user_id = "10001";
    public string appid = "001";
    public string appname = "3rdPartyGame";
    public string appdescribe = "This is just a 3rd party game.";

    string serverURL_request = "http://www.pluginx.cc/?m=sm&a=get_sm_msg";
    string serverURL_verify = "http://www.pluginx.cc/?m=sm&a=verify";
    string sdk_version = "1.0.0";
    string firmwareVersion = null;
    string hardwareVersion = null;
    string mac = null;
    string token = null;
    string sign1 = null;
    string sign2 = null;

    public AR_Gun GetGunState()
    {
        return m_arGun;
    }

    public void StartSDK(Action _shootHandler, Action _complete = null)
    {
        StartCoroutine(CoStartSDK(_shootHandler, _complete));
    }

    public void RegisterDevice(Action _complete = null)
    {
        StartCoroutine(CoRegisterDevice(_complete));
    }
    
    IEnumerator CoStartSDK(Action _shootHandler, Action _complete)
	{
        DontDestroyOnLoad(gameObject);

        ShootHandler = _shootHandler;

        //  初始化蓝牙
        InitBluetooth();
        yield return new WaitUntil(() => { return (null != mac); });

        //  订阅控制通道
        yield return StartCoroutine(Subscribe("FFF0", "FFF3", Handler_AR_Gun));

        if (null != _complete)
        {
            _complete();
        }
    }

    IEnumerator CoRegisterDevice(Action _complete)
    {
        //  读取固件和硬件版本号
        GetHardwareInfoFake(mac);
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });
        //  发起验签请求
        string msg = mac + "*"
                     + user_id + "*"
                     + sdk_version + "*"
                     + firmwareVersion + "*"
                     + hardwareVersion + "*"
                     + appid + "*"
                     + appname + "*"
                     + appdescribe;
        yield return StartCoroutine(RequestVerify(serverURL_request, msg));
        //  订阅签名通道
        yield return StartCoroutine(Subscribe("FFF0", "FFF9", ParseSign));
        //  将 token 转发给硬件
        if (null != token)
        {
            byte[] pack1 = null;
            byte[] pack2 = null;
            ParseToken(token, out pack1, out pack2);
            SendToken(pack1, pack2);
        }
        //  等待硬件返回签名包
        yield return new WaitUntil(() => { return ((null != sign1) && (null != sign2)); });
        yield return StartCoroutine(UnSubscribe("FFF0", "FFF9"));
        string new_token = mac.Replace(":", "") + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "") + token.Substring(0, 40);
        Debug.Log("str: " + new_token);
        Debug.Log("sign 1: " + sign1);
        Debug.Log("sign 2: " + sign2);
        yield return StartCoroutine(SendSign(serverURL_verify, new_token, sign1, sign2, token));

        if (null != _complete)
        {
            _complete();
        }
    }

    //  AR Gun 的消息处理函数
    bool lastTriggerDown = false;
    void Handler_AR_Gun(byte[] _data)
    {
        //Debug.Log("Gun Msg: " + BytesToHexString(_data, ":"));

        if (0x01 == _data[0])
        {
            m_arGun.triggerDown = true;
            if (false == lastTriggerDown)
            {
                if (null != ShootHandler)
                {
                    ShootHandler();
                }
            }
        }
        else
        {
            m_arGun.triggerDown = false;
        }
        lastTriggerDown = m_arGun.triggerDown;

        //  25 - 7A - B0
        if (_data[3] > 0x7A)
        {
            m_arGun.joyStickX = -(float)(_data[3] - 0x7A) / (0xB0 - 0x7A);
        }
        else
        {
            m_arGun.joyStickX = -(float)(_data[3] - 0x7A) / (0x7A - 0x25);
        }

        //  4D - 7B - C9
        if (_data[2] > 0x7B)
        {
            m_arGun.joyStickY = (float)(_data[2] - 0x7B) / (0xC9 - 0x7B);
        }
        else
        {
            m_arGun.joyStickY = (float)(_data[2] - 0x7B) / (0x7B - 0x4D);
        }
    }

    void InitBluetooth()
    {
        Debug.Log("Bluetooth Initializing...");
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Bluetooth Initialized.");
            Scan(m_hardwareName);
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
                mac = _mac;
            }
        }, null, (str) =>
        {
            Debug.Log("Disconnected.");
        });
	}

    void GetHardwareInfo(string _mac)
    {
        Debug.Log("Get hardware info from: " + _mac);

        BluetoothLEHardwareInterface.ReadCharacteristic(_mac, "FFC0", "FFC4", (Test04, data) =>
        {
            firmwareVersion = GetFirmwareVersion(data);
            hardwareVersion = GetHardwareVersion(data);
        });
    }

    void GetHardwareInfoFake(string _mac)
    {
        Debug.Log("Get hardware info(fake) from: " + _mac);

        firmwareVersion = "00.00.00.01";
        hardwareVersion = "00.01";
    }

    string GetFirmwareVersion(byte[] _data)
    {
        byte[] temp = new byte[4];

        for (int i = 0; i < temp.Length; ++i)
        {
            temp[i] = _data[i + 2];
        }

        return BytesToHexString(temp, ".");
    }

    string GetHardwareVersion(byte[] _data)
    {
        byte[] temp = new byte[2];

        for (int i = 0; i < temp.Length; ++i)
        {
            temp[i] = _data[i + 8];
        }

        return BytesToHexString(temp, ".");
    }

    //  将分段后的 token 打包为数据包，准备发送，每段 token 为 10 字节
    byte[] TokenPackage(string _tokenSubstring)
    {
        byte[] package = new byte[14];

        package[0] = 0xFE;
        package[1] = 0x0E;
        package[2] = 0xFD;

        for (int i = 0; i < 10; ++i)
        {
            package[i + 3] = Convert.ToByte(_tokenSubstring.Substring(i * 2, 2), 16);
        }
        package[13] = GetCheckSum(package);

        return package;
    }

    void ParseToken(string s, out byte[] _pack1, out byte[] _pack2)
    {
        string tokenSubstring1 = s.Substring(0, 20);
        string tokenSubstring2 = s.Substring(20, 20);

        _pack1 = new byte[14];
        _pack2 = new byte[14];
        _pack1 = TokenPackage(tokenSubstring1);
        _pack2 = TokenPackage(tokenSubstring2);
    }

    byte GetCheckSum(byte[] dateByte)
    {
        byte byteTemp;
        int sum = 0;
        for (int j = 0; j < dateByte.Length - 1; j++)
        {
            string str = dateByte[j].ToString("x");
            sum += Convert.ToInt32(str, 16);
        }
        string sum16 = Convert.ToString(sum, 16);
        int sum16Length = sum16.Length;
        if (sum16Length >= 2)
        {
            byteTemp = (byte)Convert.ToInt32(sum16.Substring(sum16Length - 2), 16);
        }
        else
        {
            byteTemp = (byte)Convert.ToInt32((sum16), 16);
        }

        return byteTemp;
    }

    void SendToken(byte[] _pack1, byte[] _pack2)
    {
        BluetoothLEHardwareInterface.WriteCharacteristic(mac, "FFF0", "FFF8", _pack1, _pack1.Length, true, (createAction) =>
        {
            BluetoothLEHardwareInterface.WriteCharacteristic(mac, "FFF0", "FFF8", _pack2, _pack2.Length, true, null);
        });
    }

    string BytesToHexString(byte[] _data, string _splitChar)
	{
		string hexString = string.Empty;
		if (_data != null)
        {
			StringBuilder strB = new StringBuilder();
			for (int i = 0; i < _data.Length; i++)
            {
				if (i != _data.Length - 1)
                {
					strB.Append (_data [i].ToString ("X2") + _splitChar);
				}
                else
                {
					strB.Append (_data [i].ToString ("X2"));
				}
			}
			hexString = strB.ToString ();
		}
		return hexString;
	}

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

    IEnumerator Subscribe(string _service, string _channel, Action<byte[]> _handler)
    {
        Debug.Log("Start Subscribe.");
        subscribeHandlers.Add(_channel.ToUpper(), _handler);
        bool complete = false;
        BluetoothLEHardwareInterface.SubscribeCharacteristic(mac, _service, _channel, (str) => 
        {
            Debug.Log("Subscribe notification: " + str);
            complete = true;
        }, SubscribeHandler);
        yield return new WaitUntil(() => complete);
	}
    
    //  TODO: 是否需要修改？
    IEnumerator UnSubscribe(string _service, string _channel)
    {
        bool complete = false;
        if (subscribeHandlers.ContainsKey(_channel.ToUpper()))
        {
            subscribeHandlers.Remove(_channel.ToUpper());
        }
        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(mac, _service, _channel, (str) => { complete = true; });
        yield return new WaitUntil(() => complete);
    }

#endregion

    int signPackCount = 0;
    string signData = "";
    void ParseSign(byte[] _data)
    {
        signPackCount++;
        signData += BytesToHexString(_data, "");
        Debug.Log("sign Data " + signPackCount + " : " + BytesToHexString(_data, ""));

        if (4 == signPackCount)
        {
            sign1 = signData.Substring(0, 64);
            sign2 = signData.Substring(64, 64);
        }
    }
    

    IEnumerator RequestVerify(string _url, string _msg)
    {
        Debug.Log("request: " + _msg);

        WWWForm form = new WWWForm();
        form.AddField("msg", _msg);
        //form.AddField("request", _msg);

        using (UnityWebRequest www = UnityWebRequest.Post(_url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                token = www.downloadHandler.text;
                Debug.Log("Token: " + token.Substring(0, 40));
            }
        }
    }

    IEnumerator SendSign(string _url, string _newtoken, string _sign1, string _sign2, string _oldtoken)
    {
        WWWForm form = new WWWForm();

        form.AddField("newtoken", _newtoken);
        form.AddField("sign1", _sign1);
        form.AddField("sign2", _sign2);
        form.AddField("oldtoken", _oldtoken);
        
        using (UnityWebRequest www = UnityWebRequest.Post(_url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Complete!: " + www.downloadHandler.text);
            }
        }
    }
}
