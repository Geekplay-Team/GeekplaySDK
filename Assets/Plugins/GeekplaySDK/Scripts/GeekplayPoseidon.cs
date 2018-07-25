using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseidonState
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

public class GeekplayPoseidon : GeekplayDevice
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

    //  _time:  0 - 25.5 秒，精度 0.1 秒
    public void MotorRun(float _time)
    {

    }

    //public void IR_SendMsg(byte _msg)
    //{
    //    BluetoothLEHardwareInterface.WriteCharacteristic(m_deviceID, "FFF0", "FFFA", _pack1, _pack1.Length, true, (createAction) =>
    //    {
    //        BluetoothLEHardwareInterface.WriteCharacteristic(m_deviceID, "FFF0", "FFFA", _pack2, _pack2.Length, true, null);
    //    });
    //}

    //  AR Gun 的消息处理函数
    bool lastTriggerDown = false;
    protected override void MsgHandler(byte[] _data)
    {
        Debug.Log("Poseidon Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));

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
