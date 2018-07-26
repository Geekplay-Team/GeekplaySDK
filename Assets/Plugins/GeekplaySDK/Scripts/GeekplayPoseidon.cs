using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseidonState
{
    public bool triggerDown = false;
    public bool pumped = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

public class GeekplayPoseidon : GeekplayDevice
{
    PoseidonState m_state = new PoseidonState();
    Action GunShootHandler = null;
    Action PumpHandler = null;

    public PoseidonState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _shootHandler = null, Action _pumpHandler = null, Action _complete = null)
    {
        RegisterCallback(_shootHandler, _pumpHandler);
        StartCoroutine(CoInitialize(m_deviceID, _complete));
    }

    public void RegisterCallback(Action _shootHandler, Action _pumpHandler)
    {
        GunShootHandler = _shootHandler;
        PumpHandler = _pumpHandler;
    }

    //  _time:  0 - 25 秒，精度 0.1 秒
    public void MotorRun(float _time)
    {
        if (_time < 0)
        {
            _time = 0;
        }
        else if (_time > 25)
        {
            _time = 25;
        }
        byte cmdTime = Convert.ToByte(_time * 10);
        byte[] pack = Package(0x61, new byte[] { cmdTime });
        WriteCharacteristic("FFF0", "FFFA", pack);
    }

    public void IR_SendMsg(byte _msg)
    {
        byte[] pack = Package(0xD0, new byte[] { _msg });
        WriteCharacteristic("FFF0", "FFFA", pack);
    }

    //  AR Gun 的消息处理函数
    bool lastTriggerDown = false;
    bool lastPumped = false;
    protected override void MsgHandler(byte[] _data)
    {
        //Debug.Log("Poseidon Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));

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

        if (0x01 == _data[4])
        {
            m_state.pumped = true;
            if (false == lastPumped)
            {
                if (null != PumpHandler)
                {
                    PumpHandler();
                }
            }
        }
        else
        {
            m_state.pumped = false;
        }
        lastPumped = m_state.pumped;

        //  最小值：0x01    归中值：0x80    最大值：0xFF
        m_state.joyStickX = (float)(_data[3] - 0x80) / 0x7F;
        m_state.joyStickY = (float)(_data[2] - 0x80) / 0x7F;
    }
}
