using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSDK : MonoBehaviour
{
    void Start()
    {
        GeekplaySDK sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();
        sdk.StartSDK(RegisterCallbacks);
    }

    void RegisterCallbacks(GeekplayDevice _device)
    {
        Debug.Log(_device.GetType());
        if (typeof(GeekplayARGun) == _device.GetType())
        {
            ((GeekplayARGun)_device).Initialize(GunShoot);
        }
        else if (typeof(GeekplayARcher) == _device.GetType())
        {
            ((GeekplayARcher)_device).Initialize(BowDraw, BowShoot, null, null);
        }
        else if (typeof(GeekplayNewARcher) == _device.GetType())
        {
            ((GeekplayNewARcher)_device).Initialize(BowDraw, BowShoot); 
        }
        else
        {
            //  do nothing
        }
    }

    void GunShoot()
    {
        Debug.Log("Gun Shoot: " + Time.time);
        //Debug.Log("X: " + gun.GetState().joyStickX);
        //Debug.Log("Y: " + gun.GetState().joyStickY);
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
