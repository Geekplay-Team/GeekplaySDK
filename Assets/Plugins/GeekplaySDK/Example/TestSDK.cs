using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSDK : MonoBehaviour
{
    GeekplaySDK sdk = null;

    GeekplayNewARcher newBow = null;
    GeekplayARcher bow = null;
    GeekplayARGun gun = null;

    void Start()
    {
        sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();
        
        if (DeviceName.AR_Gun == sdk.m_deviceName)
        {
            gun = sdk.GetDevice() as GeekplayARGun;
            gun.Initialize(GunShoot);
        }
        else if (DeviceName.ARcher == sdk.m_deviceName)
        {
            bow = sdk.GetDevice() as GeekplayARcher;
            bow.Initialize(BowDraw, BowShoot, null, null);
        }
        else if (DeviceName.New_ARcher == sdk.m_deviceName)
        {
            newBow = sdk.GetDevice() as GeekplayNewARcher;
            newBow.Initialize(BowDraw, BowShoot);
        }
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
    }
}
