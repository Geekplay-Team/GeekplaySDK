using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonboneState
{
    public bool stringPulled = false;
    public int drawLength = 0;
    public const float fullDrawLength = 75.0f;
}

public class GeekplayDragonbone : GeekplayDevice
{
    DragonboneState m_state = new DragonboneState();
    Action DrawHandler = null;
    Action ShootHandler = null;

    public DragonboneState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _complete = null)
    {
        RegisterCallback(_drawHandler, _shootHandler);
        StartCoroutine(CoInitialize(m_deviceID, _complete));
    }

    public void RegisterCallback(Action _drawHandler = null, Action _shootHandler = null)
    {
        DrawHandler = _drawHandler;
        ShootHandler = _shootHandler;
    }

    //  New ARcher 的消息处理函数
    bool lastStringPulled = false;
    protected override void MsgHandler(byte[] _data)
    {
        //Debug.Log("New ARcher Msg: " + GeekplayCommon.BytesToHexString(_data, ":"));
        //  切换大小端
        byte temp = _data[11];
        _data[11] = _data[12];
        _data[12] = temp;

        //  初始位置：0      极限拉距：127        成人正常满幅拉距：75
        m_state.drawLength = BitConverter.ToInt16(_data, 11);
        Debug.Log("Draw Length: " + m_state.drawLength);
        //  施密特触发
        const int lowThreshold = 20;
        const int highThreshold = 30;

        if (true == lastStringPulled)
        {
            if (m_state.drawLength < lowThreshold)
            {
                lastStringPulled = false;
                if (null != ShootHandler)
                {
                    ShootHandler();
                }
            }
        }
        else
        {
            if (m_state.drawLength > highThreshold)
            {
                lastStringPulled = true;
                if (null != DrawHandler)
                {
                    DrawHandler();
                }
            }
        }
    }
}
