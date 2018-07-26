using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class GeekplayDevice : MonoBehaviour
{
    protected bool isLegal = false;
    public string m_deviceID = null;
    public string m_MAC = null;

    bool connected = false;

    string m_userID = null;
    string m_appID = null;
    string m_appName = null;
    string m_appDescription = null;

    #region 初始化

    public void SetAppInfo(string _userID, string _appID, string _appName, string _appDescription)
    {
        m_userID = _userID;
        m_appID = _appID;
        m_appName = _appName;
        m_appDescription = _appDescription;
    }

    protected IEnumerator CoInitialize(string _deviceID, Action _complete)
    {
        //  若多次调用，则只执行一次
        if (!connected)
        {
            Connect(_deviceID);
            yield return new WaitUntil(() => connected);
            yield return StartCoroutine(CoVerifyDevice());
            yield return new WaitUntil(() => isLegal);
            //  订阅控制通道
            yield return StartCoroutine(Subscribe(m_deviceID, "FFF0", "FFF8", MsgHandler));
            gameObject.GetComponent<AudioSource>().Play();

            if (null != _complete)
            {
                _complete();
            }
        }
    }

    protected abstract void MsgHandler(byte[] _data);


    bool FFF0_enabled = false;
    bool FFC0_enabled = false;
    void Connect(string _deviceID, Action _complete = null)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceID, (str) =>
        {
            Debug.Log("Connected: " + str);
            if (null != _complete)
            {
                _complete();
            }
        }, (address, service) =>
        {
#if UNITY_ANDROID
            service = service.Substring(4, 4).ToUpper();
#endif
            //  等待所有服务开启后，就可以进行下一步（FFC0 和 FFF0）
            //  TODO: 未完成
            if ("FFF0" == service)
            {
                FFF0_enabled = true;
            }
            else if ("FFC0" == service)
            {
                FFC0_enabled = true;
            }
            if (FFC0_enabled && FFF0_enabled)
            {
                connected = true;
            }
        }, null, (str) =>
        {
            if (isLegal)
            {
                Debug.Log("Reconnecting :" + str);
                connected = false;
                Connect(str, () => { StartCoroutine(Subscribe(m_deviceID, "FFF0", "FFF8", MsgHandler)); });
            }
        });
    }

    #endregion

    #region 订阅 notify 通道
    Dictionary<string, Action<byte[]>> subscribeHandlers = new Dictionary<string, Action<byte[]>>();
    void SubscribeHandler(string _deviceID, string _channel, byte[] _data)
    {
#if UNITY_ANDROID
        //  iOS 的通道号为大写，Android 为小写，因此统一为大写处理
        _channel = _channel.ToUpper().Substring(4, 4);
#endif
        if ((subscribeHandlers.ContainsKey(_channel)) && null != subscribeHandlers[_channel])
        {
            subscribeHandlers[_channel](_data);
        }
    }

    public IEnumerator Subscribe(string _deviceID, string _service, string _channel, Action<byte[]> _handler)
    {
        //  TODO: 去掉延时，改为等待某个特定事件
        yield return new WaitForSeconds(0.5f);
        _channel = _channel.ToUpper();
        Debug.Log("Start Subscribe Service " + _service + ", Channel " + _channel);
        if (!subscribeHandlers.ContainsKey(_channel))
        {
            //  新增
            subscribeHandlers.Add(_channel, _handler);
        }
        else
        {
            //  替换
            subscribeHandlers[_channel] = _handler;
        }
        bool complete = false;
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceID, _service, _channel, (deviceID, characteristic) =>
        {
            Debug.Log("Subscribe " + characteristic + " of " + deviceID);
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
        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(m_deviceID, _service, _channel, (str) => { complete = true; });
        yield return new WaitUntil(() => complete);
    }
    #endregion

    #region 验证设备合法性

    string pubKeyX = "C0FFECBC37E9D4AE387896773C148ED6F4B65ABDC62BE8174891C0C4DB54A342";
    string pubKeyY = "47C51B264393254212424C0F9825A565158C179F98014117228E7C1C2BF5E9F1";

    string serverURL_request = "http://www.pluginx.cc/?m=sm&a=get_sm_msg";
    string serverURL_verify = "http://www.pluginx.cc/?m=sm&a=verify";
    string sdk_version = "0.6.0";
    string firmwareVersion = null;
    string hardwareVersion = null;

    string token = null;    //  20 bytes 的 token
    string sign1 = null;
    string sign2 = null;
    
    public void RegisterDevice(Action _complete = null)
    {
        StartCoroutine(CoRegisterDevice(_complete));
    }

    IEnumerator CoRegisterDevice(Action _complete)
    {
        //  读取固件和硬件版本号
        GetHardwareInfo(m_deviceID);
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });
        //  发起验签请求
        string msg = m_MAC + "*"
                     + m_userID + "*"
                     + sdk_version + "*"
                     + firmwareVersion + "*"
                     + hardwareVersion + "*"
                     + m_appID + "*"
                     + m_appName + "*"
                     + m_appDescription;
        yield return StartCoroutine(RequestVerify(serverURL_request, msg));
        //  订阅签名通道
        yield return StartCoroutine(Subscribe(m_deviceID, "FFF0", "FFF9", ParseSign4Remote));
        //  将 token 转发给硬件
        if (null != token)
        {
            //  将 token 分为两段并打包为固件可解析的格式
            byte[] pack1 = Package(0xFD, token.Substring(0, 20));
            byte[] pack2 = Package(0xFD, token.Substring(20, 20));
            WriteCharacteristic("FFF0", "FFFA", pack1);
            WriteCharacteristic("FFF0", "FFFA", pack2);
        }
        //  等待硬件返回签名包
        yield return new WaitUntil(() => { return ((null != sign1) && (null != sign2)); });
        yield return StartCoroutine(UnSubscribe("FFF0", "FFF9"));
        string new_token = m_deviceID.Replace(":", "") + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "") + token.Substring(0, 40);
        Debug.Log("str: " + new_token);
        Debug.Log("sign 1: " + sign1);
        Debug.Log("sign 2: " + sign2);
        yield return StartCoroutine(SendSign(serverURL_verify, new_token, sign1, sign2, token));

        if (null != _complete)
        {
            _complete();
        }
    }

    byte[] GenerateTokenPack()
    {
        byte[] pack = new byte[10];
        for (int i = 0; i < 10; ++i)
        {
            pack[i] = (byte)UnityEngine.Random.Range(0, 255);
        }
        return Package(0xFD, pack);
    }

    string signPack = null;
    protected IEnumerator CoVerifyDevice(Action _complete = null)
    {
        //  读取固件和硬件版本号
        GetHardwareInfo(m_deviceID);
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });

        //  订阅签名通道
        yield return StartCoroutine(Subscribe(m_deviceID, "FFF0", "FFF9", ParseSign4Local));

        //  将 token 发给硬件
        byte[] tokenPack1 = GenerateTokenPack();
        byte[] tokenPack2 = GenerateTokenPack();
        WriteCharacteristic("FFF0", "FFFA", tokenPack1);
        WriteCharacteristic("FFF0", "FFFA", tokenPack2);

        //  等待硬件返回签名包
        yield return new WaitUntil(() => { return (null != signPack); });
        Debug.Log("Sign Received.");
        yield return StartCoroutine(UnSubscribe("FFF0", "FFF9"));
        Debug.Log("signPack: " + signPack);
        string dataToSign = m_MAC + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "")
                            + GeekplayCommon.BytesToHexString(tokenPack1, "").Substring(6, 20)
                            + GeekplayCommon.BytesToHexString(tokenPack2, "").Substring(6, 20);
        Debug.Log("Data to sign: " + dataToSign);

        if (GeekplayCommon.VerifySign(dataToSign, signPack, pubKeyX, pubKeyY))
        {
            Debug.Log("Legal Hardware!");
            isLegal = true;
        }
        else
        {
            Debug.Log("Illegal Hardware!");
            isLegal = false;
        }

        if (null != _complete)
        {
            _complete();
        }
    }

    void GetHardwareInfo(string _mac)
    {
        Debug.Log("Get hardware info from: " + _mac);

        BluetoothLEHardwareInterface.ReadCharacteristic(_mac, "FFC0", "FFC4", (_channel, _data) =>
        {
#if UNITY_ANDROID
            _channel = _channel.Substring(4, 4).ToUpper();
#endif
            if ("FFC4" == _channel)
            {
                firmwareVersion = GetFirmwareVersion(_data);
                hardwareVersion = GetHardwareVersion(_data);
                Debug.Log("Firmware Version: " + firmwareVersion);
                Debug.Log("Hardware Version: " + hardwareVersion);
            }
        });
    }

    string GetFirmwareVersion(byte[] _data)
    {
        byte[] temp = new byte[4];

        for (int i = 0; i < temp.Length; ++i)
        {
            temp[i] = _data[i + 2];
        }

        return GeekplayCommon.BytesToHexString(temp, ".");
    }

    string GetHardwareVersion(byte[] _data)
    {
        byte[] temp = new byte[2];

        for (int i = 0; i < temp.Length; ++i)
        {
            temp[i] = _data[i + 8];
        }

        return GeekplayCommon.BytesToHexString(temp, ".");
    }

    int signPackCount4Remote = 0;
    string signData4Remote = "";
    void ParseSign4Remote(byte[] _data)
    {
        signPackCount4Remote++;
        signData4Remote += GeekplayCommon.BytesToHexString(_data, "");
        Debug.Log("sign Data " + signPackCount4Remote + " : " + GeekplayCommon.BytesToHexString(_data, ""));

        if (4 == signPackCount4Remote)
        {
            signPackCount4Remote = 0;
            sign1 = signData4Remote.Substring(0, 64);
            sign2 = signData4Remote.Substring(64, 64);
            signData4Remote = "";
        }
    }

    int signPackCount4Local = 0;
    string signData4Local = "";
    void ParseSign4Local(byte[] _data)
    {
        signPackCount4Local++;
        signData4Local += GeekplayCommon.BytesToHexString(_data, "");
        Debug.Log("sign Data " + signPackCount4Local + " : " + GeekplayCommon.BytesToHexString(_data, ""));

        if (4 == signPackCount4Local)
        {
            signPackCount4Local = 0;
            signPack = signData4Local;
            signData4Local = "";
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

    #endregion


    #region 通信相关功能函数

    protected byte[] Package(byte _cmdType, byte[] _cmd)
    {
        int length = _cmd.Length + 4;
        byte[] package = new byte[length];

        package[0] = 0xFE;
        package[1] = Convert.ToByte(length);
        package[2] = _cmdType;

        for (int i = 0; i < _cmd.Length; ++i)
        {
            package[i + 3] = _cmd[i];
        }
        package[length - 1] = GetCheckSum(package);

        return package;
    }

    protected byte[] Package(byte _cmdType, string _cmd)
    {
        int length = _cmd.Length / 2 + 4;
        byte[] package = new byte[length];
        package[2] = _cmdType;

        for (int i = 0; i < _cmd.Length; ++i)
        {
            package[i + 3] = Convert.ToByte(_cmd.Substring(i * 2, 2), 16);
        }
        package[length - 1] = GetCheckSum(package);

        return package;
    }

    //  计算校验和
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

    
    List<byte[]> msgToWrite = new List<byte[]>();
    bool listSendComplete = true;
    protected void WriteCharacteristic(string _service, string _characteristic, byte[] _data)
    {
        msgToWrite.Add(_data);
        if (listSendComplete)
        {
            StartCoroutine(CoWriteCharacteristic(_service, _characteristic));
        }
    }

    IEnumerator CoWriteCharacteristic(string _service, string _characteristic)
    {
        listSendComplete = false;
        bool complete = true;
        while (msgToWrite.Count > 0)
        {
            complete = false;
            BluetoothLEHardwareInterface.WriteCharacteristic(m_deviceID, _service, _characteristic, msgToWrite[0], msgToWrite[0].Length, true, (str) =>
            {
                msgToWrite.RemoveAt(0);
                complete = true;
            });
            yield return new WaitUntil(() => complete);
        }
        listSendComplete = true;
    }

    #endregion

}
