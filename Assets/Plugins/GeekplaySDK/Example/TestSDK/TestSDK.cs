using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSDK : MonoBehaviour
{
    GeekplayDevice m_device;
    public Camera m_camera;
    public Transform m_cameraOrigin;

    void Start()
    {
        GeekplaySDK sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();
        sdk.StartSDK(RegisterCallbacks);
    }

    void RegisterCallbacks(GeekplayDevice _device)
    {
        Debug.Log(_device.GetType());
        if (typeof(GeekplayElite) == _device.GetType())
        {
            ((GeekplayElite)_device).Initialize(GunShoot);
        }
        else if (typeof(GeekplayPoseidon) == _device.GetType())
        {
            ((GeekplayPoseidon)_device).Initialize(GunShoot, GunPump);
        }
        else if (typeof(GeekplayHunter) == _device.GetType())
        {
            ((GeekplayHunter)_device).Initialize(BowDraw, BowShoot, ButtonPressed, ButtonReleased);
        }
        else if (typeof(GeekplayDragonbone) == _device.GetType())
        {
            ((GeekplayDragonbone)_device).Initialize(BowDraw, BowShoot); 
        }
        else if (typeof(GeekplayMR_Camera) == _device.GetType())
        {
            ((GeekplayMR_Camera)_device).Initialize(m_camera, new Vector3(0, 0, 0));
        }

        m_device = _device;
    }

    void GunShoot()
    {
        Debug.Log("Gun Shoot: " + Time.time);

        if (typeof(GeekplayPoseidon) == m_device.GetType())
        {
            GeekplayPoseidon gun = (GeekplayPoseidon)m_device;
            gun.MotorRun(0.3f);
            gun.IR_SendMsg(0xCC);
            Debug.Log("X: " + gun.GetState().joyStickX);
            Debug.Log("Y: " + gun.GetState().joyStickY);
        }
    }

    void GunPump()
    {
        Debug.Log("Gun Pump: " + Time.time);
    }

    void BowDraw()
    {
        Debug.Log("Bow Draw: " + Time.time);
    }

    void BowShoot()
    {
        Debug.Log("Bow Shoot: " + Time.time);
    }

    void ButtonPressed()
    {
        Debug.Log("Button Pressed: " + Time.time);
    }

    void ButtonReleased()
    {
        Debug.Log("Button Released: " + Time.time);
    }
}
