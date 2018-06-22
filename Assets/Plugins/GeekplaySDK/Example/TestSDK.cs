using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSDK : MonoBehaviour
{
    GeekplaySDK sdk = null;

    GeekplayARcher bow = null;
    GeekplayARGun gun = null;

    void Start()
    {
        sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();
        sdk.StartSDK();

        if (DeviceName.ARGUN == sdk.m_deviceName)
        {
            gun = sdk.GetDevice() as GeekplayARGun;
            gun.Initialize(GunShoot, Register);
        }
        else if (DeviceName.ARCHER == sdk.m_deviceName)
        {
            bow = sdk.GetDevice() as GeekplayARcher;
            bow.Initialize(BowDraw, BowShoot, null, null, Register);
        }
    }

    void Register()
    {
        sdk.RegisterDevice();
    }

    void GunShoot()
    {
        Debug.Log("Gun Shoot: " + Time.time);
        Debug.Log("X: " + gun.GetState().joyStickX);
        Debug.Log("Y: " + gun.GetState().joyStickY);
    }

    void BowDraw()
    {
        Debug.Log("Bow Draw: " + Time.time);
    }

    void BowShoot()
    {
        Debug.Log("Bow Shoot: " + Time.time);
        Debug.Log("Button: " + bow.GetState().buttonPressed);
    }
}
