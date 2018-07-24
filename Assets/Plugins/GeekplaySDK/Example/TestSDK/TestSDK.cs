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
        if (typeof(GeekplayElite) == _device.GetType())
        {
            ((GeekplayElite)_device).Initialize(GunShoot);
        }
        else if (typeof(GeekplayPoseidon) == _device.GetType())
        {
            ((GeekplayPoseidon)_device).Initialize(GunShoot);
        }
        else if (typeof(GeekplayHunter) == _device.GetType())
        {
            ((GeekplayHunter)_device).Initialize(BowDraw, BowShoot, ButtonPressed, ButtonReleased);
        }
        else if (typeof(GeekplayDragonbone) == _device.GetType())
        {
            ((GeekplayDragonbone)_device).Initialize(BowDraw, BowShoot); 
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

    void ButtonPressed()
    {
        Debug.Log("Button Pressed: " + Time.time);
    }

    void ButtonReleased()
    {
        Debug.Log("Button Released: " + Time.time);
    }
}
