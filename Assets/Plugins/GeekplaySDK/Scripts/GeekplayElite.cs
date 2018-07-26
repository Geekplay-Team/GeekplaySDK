using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteState
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

public class GeekplayElite : GeekplayDevice
{
    EliteState m_state = new EliteState();
    Action GunShootHandler = null;

    public EliteState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _shootHandler = null, Action _complete = null)
    {
        RegisterCallback(_shootHandler);
        StartCoroutine(CoInitialize(m_deviceID, _complete));
    }

    public void RegisterCallback(Action _shootHandler)
    {
        GunShootHandler = _shootHandler;
    }

    //  AR Gun 的消息处理函数
    bool lastTriggerDown = false;
    protected override void MsgHandler(byte[] _data)
    {
        //Debug.Log("Elite Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));

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

        //  最小值：0x01    归中值：0x80    最大值：0xFF
        m_state.joyStickX = (float)(_data[3] - 0x80) / 0x7F;
        m_state.joyStickY = (float)(_data[2] - 0x80) / 0x7F;
    }
}
