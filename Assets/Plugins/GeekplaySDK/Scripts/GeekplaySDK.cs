using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public enum DeviceName
{
    ARGUN = 1, 
    ARCHER = 2
}

public class GeekplaySDK : MonoBehaviour
{
    string pubKeyX = "C0FFECBC37E9D4AE387896773C148ED6F4B65ABDC62BE8174891C0C4DB54A342";
    string pubKeyY = "47C51B264393254212424C0F9825A565158C179F98014117228E7C1C2BF5E9F1";

    public DeviceName m_deviceName;
    GeekplayDevice m_device = null;
    
    public string user_id = "10001";
    public string appid = "001";
    public string appname = "3rdPartyGame";
    public string appdescribe = "This is just a 3rd party game.";

    string serverURL_request = "http://www.pluginx.cc/?m=sm&a=get_sm_msg";
    string serverURL_verify = "http://www.pluginx.cc/?m=sm&a=verify";
    string sdk_version = "1.0.0";
    string firmwareVersion = null;
    string hardwareVersion = null;

    string token = null;    //  20 bytes 的 token
    string sign1 = null;
    string sign2 = null;

    public GeekplayDevice GetDevice()
    {
        return m_device;
    }

    public void StartSDK()
    {
        DontDestroyOnLoad(gameObject);

        if (DeviceName.ARGUN == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARGun>();
        }
        else if (DeviceName.ARCHER == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARcher>();
        }
        else
        {
            m_device = null;
        }
    }

    public void RegisterDevice(Action _complete = null)
    {
        StartCoroutine(CoRegisterDevice(_complete));
    }

    IEnumerator CoRegisterDevice(Action _complete)
    {
        //  读取固件和硬件版本号
        GetHardwareInfo(m_device.GetMAC());
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });
        //  发起验签请求
        string msg = m_device.GetMAC() + "*" 
                     + user_id + "*" 
                     + sdk_version + "*" 
                     + firmwareVersion + "*" 
                     + hardwareVersion + "*" 
                     + appid + "*" 
                     + appname + "*" 
                     + appdescribe;
        yield return StartCoroutine(RequestVerify(serverURL_request, msg));
        //  订阅签名通道
        yield return StartCoroutine(m_device.Subscribe("FFF0", "FFF9", ParseSign4Remote));
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
        yield return StartCoroutine(m_device.UnSubscribe("FFF0", "FFF9"));
        string new_token = m_device.GetMAC().Replace(":", "") + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "") + token.Substring(0, 40);
        Debug.Log("str: " + new_token);
        Debug.Log("sign 1: " + sign1);
        Debug.Log("sign 2: " + sign2);
        yield return StartCoroutine(SendSign(serverURL_verify, new_token, sign1, sign2, token));

        if (null != _complete)
        {
            _complete();
        }
    }

    public void VerifyDevice(Action _complete = null)
    {
        StartCoroutine(CoVerifyDevice(_complete));
    }

    byte[] GenerateTokenPack()
    {
        byte[] pack = new byte[10];
        for (int i = 0; i < 10; ++i)
        {
            pack[i] = (byte)UnityEngine.Random.Range(0, 255);
        }
        return TokenPackage(GeekplayCommon.BytesToHexString(pack, ""));
    }

    string signPack = null;
    IEnumerator CoVerifyDevice(Action _complete)
    {
        //  读取固件和硬件版本号
        GetHardwareInfo(m_device.GetMAC());
        yield return new WaitUntil(() => { return ((null != firmwareVersion) && (null != hardwareVersion)); });

        //  订阅签名通道
        yield return StartCoroutine(m_device.Subscribe("FFF0", "FFF9", ParseSign4Local));

        //  将 token 发给硬件
        byte[] tokenPack1 = GenerateTokenPack();
        byte[] tokenPack2 = GenerateTokenPack();
        SendToken(tokenPack1, tokenPack2);

        //  等待硬件返回签名包
        yield return new WaitUntil(() => { return (null != signPack); });
        Debug.Log("Sign Received.");
        yield return StartCoroutine(m_device.UnSubscribe("FFF0", "FFF9"));
        Debug.Log("signPack: " + signPack);
        string dataToSign = m_device.GetMAC().Replace(":", "") + firmwareVersion.Replace(".", "") + hardwareVersion.Replace(".", "")
                            + GeekplayCommon.BytesToHexString(tokenPack1, "").Substring(6, 20)
                            + GeekplayCommon.BytesToHexString(tokenPack2, "").Substring(6, 20);
        Debug.Log("Dato to sign: " + dataToSign);
        
        if (GeekplayCommon.VerifySign(dataToSign, signPack, pubKeyX, pubKeyY))
        {
            Debug.Log("Legal Hardware!");
        }
        else
        {
            Debug.Log("Illegal Hardware!");
        }

        if (null != _complete)
        {
            _complete();
        }
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

    //  将 token 分为两段并打包为固件可解析的格式
    void ParseToken(string s, out byte[] _pack1, out byte[] _pack2)
    {
        string tokenSubstring1 = s.Substring(0, 20);
        string tokenSubstring2 = s.Substring(20, 20);

        _pack1 = new byte[14];
        _pack2 = new byte[14];
        _pack1 = TokenPackage(tokenSubstring1);
        _pack2 = TokenPackage(tokenSubstring2);
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

    void SendToken(byte[] _pack1, byte[] _pack2)
    {
        BluetoothLEHardwareInterface.WriteCharacteristic(m_device.GetMAC(), "FFF0", "FFFA", _pack1, _pack1.Length, true, (createAction) =>
        {
            BluetoothLEHardwareInterface.WriteCharacteristic(m_device.GetMAC(), "FFF0", "FFFA", _pack2, _pack2.Length, true, null);
        });
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
}
