using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum DeviceName
{
    ARGUN = 1, 
    ARCHER = 2
}

public class GeekplaySDK : MonoBehaviour
{
    public DeviceName m_deviceName;
    GeekplayDevice m_device = null;
    
    public string m_userID = "10001";
    public string m_appID = "001";
    public string m_appName = "3rdPartyGame";
    public string m_appDescription = "This is just a 3rd party game.";
    

    public GeekplayDevice GetDevice()
    {
        if (null == m_device)
        {
            StartSDK();
        }
        return m_device;
    }

    void StartSDK()
    {
        DontDestroyOnLoad(gameObject);

        if (DeviceName.ARGUN == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARGun>();
            m_device.SetAppInfo(m_userID, m_appID, m_appName, m_appDescription);
        }
        else if (DeviceName.ARCHER == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARcher>();
            m_device.SetAppInfo(m_userID, m_appID, m_appName, m_appDescription);
        }
        else
        {
            m_device = null;
        }
    }
}
