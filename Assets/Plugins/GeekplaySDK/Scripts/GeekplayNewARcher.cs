using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewARcherState
{
    public bool stringPulled = false;
    public int drawLength = 0;
}

public class GeekplayNewARcher : GeekplayDevice
{
    NewARcherState m_state = new NewARcherState();
    Action DrawHandler = null;
    Action ShootHandler = null;

    public NewARcherState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _complete = null)
    {
        DrawHandler = _drawHandler;
        ShootHandler = _shootHandler;
        StartCoroutine(CoInitialize("R-ARCHER", _complete));
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
