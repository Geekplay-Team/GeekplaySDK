using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class GeekplayBLE : MonoBehaviour
{
    string serverURL_request = "http://www.pluginx.cc/?m=sm&a=get_sm_msg";
    string serverURL_verify = "http://www.pluginx.cc/?m=sm&a=velify";
    string sdk_version = "1.0.0";
    string user_id = "10001";
    string appid = "001";
    string appname = "3rdPartyGame";
    string appdescribe = "This is just a 3rd party game.";
    string firmwareVersion = null;
    string hardwareVersion = null;
    string mac = null;
    string token = null;
    string sign1 = null;
    string sign2 = null;

    public void StartSDK()
    {
        StartCoroutine(CoStartSDK());
    }
    
    IEnumerator CoStartSDK()
	{
        DontDestroyOnLoad(gameObject);

        //  初始化蓝牙
        InitBluetooth();
        yield return new WaitUntil(() => { return (null != mac); });

        //  读取固件和硬件版本号
        GetHardwareInfo(mac);
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });
        //  发起验签请求
        string msg = mac + "*" + user_id + "*" + sdk_version + "*" + firmwareVersion + "*" + hardwareVersion + "*" + appid + "*" + appname + "*" + appdescribe;
        yield return StartCoroutine(RequestVerify(serverURL_request, msg));
        //  订阅签名通道
        yield return StartCoroutine(Subscribe());
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
        yield return StartCoroutine(UnSubscribe());
        string new_token = mac.Replace(":", "") + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "") + token.Substring(0, 40);
        Debug.Log("str: " + new_token);
        Debug.Log("sign 1: " + sign1);
        Debug.Log("sign 2: " + sign2);
        yield return StartCoroutine(SendSign(serverURL_verify, new_token, sign1, sign2,token));
    }
    
    void InitBluetooth()
    {
        Debug.Log("Bluetooth Initializing...");
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Bluetooth Initialized.");
            Scan("GU-Geekplay");
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
            if (name.Substring(4, 4).ToUpper() == "FFC0")
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
        BluetoothLEHardwareInterface.WriteCharacteristic(mac, "FFC0", "FFC2", _pack1, _pack1.Length, true, (createAction) =>
        {
            BluetoothLEHardwareInterface.WriteCharacteristic(mac, "FFC0", "FFC2", _pack2, _pack2.Length, true, null);
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

    IEnumerator Subscribe()
    {
        Debug.Log("Start Subscribe.");
        bool complete = false;
        BluetoothLEHardwareInterface.SubscribeCharacteristic(mac, "FFC0", "FFC5", (str) => 
        {
            Debug.Log("Subscribe notification: " + str);
            complete = true;
        }, ParseSign);
        yield return new WaitUntil(() => complete);
	}

    IEnumerator UnSubscribe()
    {
        bool complete = false;
        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(mac, "FFC0", "FFC5", (str) => { complete = true; });
        yield return new WaitUntil(() => complete);
    }

    int signPackCount = 0;
    string signData = "";
    void ParseSign(string _channel, byte[] _data)
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
        //  form.AddField("tk", _token);
        // form.AddField("at1", _sign1);
        // form.AddField("at2", _sign2);
        //  TODO: 改为 verify
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
