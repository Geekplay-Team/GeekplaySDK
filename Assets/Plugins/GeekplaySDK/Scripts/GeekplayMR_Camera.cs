using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeekplayMR_Camera : GeekplayDevice
{
    public Camera m_MR_Camera;
    Vector3 m_originPos = new Vector3(0, 0, 0);

    public void Initialize(Camera _camera, Vector3 _originPos, Action _complete = null)
    {
        m_MR_Camera = _camera;
        m_originPos = _originPos;
        StartCoroutine(CoInitialize(m_deviceID, _complete));
    }

    protected override void MsgHandler(byte[] _data)
    {
        if (0x01 == _data[0])
        {
            float posX = BitConverter.ToSingle(_data, 1);
            float posY = BitConverter.ToSingle(_data, 5);
            float posZ = BitConverter.ToSingle(_data, 9);

            m_MR_Camera.transform.position = new Vector3(posX, posY, posZ) - m_originPos;
            Debug.Log("Pos: " + m_MR_Camera.transform.position);
        }
        else if (0x02 == _data[0])
        {
            float eulerX = BitConverter.ToSingle(_data, 1);
            float eulerY = BitConverter.ToSingle(_data, 5);
            float eulerZ = BitConverter.ToSingle(_data, 9);

            m_MR_Camera.transform.rotation = Quaternion.Euler(new Vector3(eulerX, eulerY, eulerZ));
            Debug.Log("Rotation: " + m_MR_Camera.transform.rotation.eulerAngles);
        }
    }
}
