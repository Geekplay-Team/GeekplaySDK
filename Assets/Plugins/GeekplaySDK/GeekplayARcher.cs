using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARcherState
{
    public bool stringPulled = false;
    public bool buttonPressed = false;
}

public class GeekplayARcher : GeekplayDevice
{
    ARcherState m_state = new ARcherState();
    Action DrawHandler = null;
    Action ShootHandler = null;
    Action PressHandler = null;
    Action ReleaseHandler = null;

    public ARcherState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _pressHandler = null, Action _releaseHandler = null, Action _complete = null)
    {
        base.Initialize();
        DrawHandler = _drawHandler;
        ShootHandler = _shootHandler;
        PressHandler = _pressHandler;
        ReleaseHandler = _releaseHandler;
        StartCoroutine(CoInitialize(_drawHandler, _shootHandler, _pressHandler, _releaseHandler, _complete));
    }

    IEnumerator CoInitialize(Action _drawHandler, Action _shootHandler, Action _pressHandler, Action _releaseHandler, Action _complete)
    {
        InitBluetooth("GU-ARCHER");
        yield return new WaitUntil(() => { return (null != m_mac); });

        //  订阅控制通道
        yield return StartCoroutine(Subscribe("FFF0", "FFF3", Handler_ARcher));

        if (null != _complete)
        {
            _complete();
        }
    }

    //  ARcher 的消息处理函数
    bool lastStringPulled = false;
    bool lastButtonPressed = false;
    void Handler_ARcher(byte[] _data)
    {
        //Debug.Log("ARcher Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));
        if (0x01 == _data[0])
        {
            m_state.stringPulled = true;
            if (false == lastStringPulled)
            {
                if (null != DrawHandler)
                {
                    DrawHandler();
                }
            }
        }
        else
        {
            m_state.stringPulled = false;
            if (true == lastStringPulled)
            {
                if (null != ShootHandler)
                {
                    ShootHandler();
                }
            }
        }
        lastStringPulled = m_state.stringPulled;

        if (0x01 == _data[1])
        {
            m_state.buttonPressed = true;
            if (false == lastButtonPressed)
            {
                if (null != PressHandler)
                {
                    PressHandler();
                }
            }
        }
        else
        {
            m_state.buttonPressed = false;
            if (true == lastButtonPressed)
            {
                if (null != ReleaseHandler)
                {
                    ReleaseHandler();
                }
            }
        }
        lastButtonPressed = m_state.buttonPressed;
    }
}
