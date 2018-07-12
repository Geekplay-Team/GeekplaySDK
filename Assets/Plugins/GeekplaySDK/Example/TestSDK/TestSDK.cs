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

        switch (sdk.m_deviceName)
        {
            case DeviceName.AR_Gun:
                gun = sdk.GetDevice() as GeekplayARGun;
                gun.Initialize(GunShoot);
                break;
            case DeviceName.ARcher:
                bow = sdk.GetDevice() as GeekplayARcher;
                bow.Initialize(BowDraw, BowShoot, null, null);
                break;
            case DeviceName.New_ARcher:
                newBow = sdk.GetDevice() as GeekplayNewARcher;
                newBow.Initialize(BowDraw, BowShoot);
                break;
            default:
                break;
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
