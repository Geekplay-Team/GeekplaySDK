using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum DeviceName
{
    AR_Gun = 1, 
    ARcher = 2, 
    New_ARcher = 3
}

public class GeekplaySDK : MonoBehaviour
{
    public DeviceName m_deviceName;
    GeekplayDevice m_device = null;
    
    public string m_userID = "10001";
    public string m_appID = "001";
    public string m_appName = "3rdPartyGame";
    public string m_appDescription = "This is just a 3rd party game.";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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
        if (DeviceName.AR_Gun == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARGun>();
        }
        else if (DeviceName.ARcher == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayARcher>();
        }
        else if (DeviceName.New_ARcher == m_deviceName)
        {
            m_device = gameObject.AddComponent<GeekplayNewARcher>();
        }
        else
        {
            m_device = null;
        }

        if (null != m_device)
        {
            m_device.SetAppInfo(m_userID, m_appID, m_appName, m_appDescription);
        }
    }
}
