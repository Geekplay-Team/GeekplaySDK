using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARGunState
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

public class GeekplayARGun : GeekplayDevice
{
    ARGunState m_state = new ARGunState();
    Action GunShootHandler = null;

    public ARGunState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _shootHandler = null, Action _complete = null)
    {
        base.Initialize();
        GunShootHandler = _shootHandler;
        StartCoroutine(CoInitialize(_shootHandler, _complete));
    }

    IEnumerator CoInitialize(Action _shootHandler, Action _complete)
    {
        InitBluetooth("GU-ARGUN");
        yield return new WaitUntil(() => { return (null != m_mac); });

        //  订阅控制通道
        yield return StartCoroutine(Subscribe("FFF0", "FFF3", Handler_AR_Gun));

        if (null != _complete)
        {
            _complete();
        }
    }

    //  AR Gun 的消息处理函数
    bool lastTriggerDown = false;
    void Handler_AR_Gun(byte[] _data)
    {
        //Debug.Log("Gun Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));

        if (0x01 == _data[0])
        {
            m_state.triggerDown = true;
            if (false == lastTriggerDown)
            {
                if (null != GunShootHandler)
                {
                    GunShootHandler();
                }
            }
        }
        else
        {
            m_state.triggerDown = false;
        }
        lastTriggerDown = m_state.triggerDown;

        //  25 - 7A - B0
        if (_data[3] > 0x7A)
        {
            m_state.joyStickX = -(float)(_data[3] - 0x7A) / (0xB0 - 0x7A);
        }
        else
        {
            m_state.joyStickX = -(float)(_data[3] - 0x7A) / (0x7A - 0x25);
        }

        //  4D - 7B - C9
        if (_data[2] > 0x7B)
        {
            m_state.joyStickY = (float)(_data[2] - 0x7B) / (0xC9 - 0x7B);
        }
        else
        {
            m_state.joyStickY = (float)(_data[2] - 0x7B) / (0x7B - 0x4D);
        }
    }
}
