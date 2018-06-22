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

    public ARcherState GetState()
    {
        return m_state;
    }

    public void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _pressHandler = null, Action _releaseHandler = null, Action _complete = null)
    {
        base.Initialize();
        StartCoroutine(CoInitialize(_drawHandler, _shootHandler, _pressHandler, _releaseHandler, _complete));
    }

    IEnumerator CoInitialize(Action _drawHandler, Action _shootHandler, Action _pressHandler, Action _releaseHandler, Action _complete)
    {
        InitBluetooth("GU-ARCHER");
        yield return new WaitUntil(() => { return (null != m_mac); });
        

        if (null != _complete)
        {
            _complete();
        }
    }
}
