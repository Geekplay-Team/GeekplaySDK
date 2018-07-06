# Geekplay SDK for Unity

## Introduction

This plugin provides basic access to Geekplay AR Gun, ARcher and AR Unit. You can use this SDK from Unity to connect Geekplay products with your own apps.

## Version Changes

| Version | Date       | Description                                         |
| ------- | ---------- | --------------------------------------------------- |
| 0.3.0   | 2018-07-05 | Added local legitimacy verification.                |
| 0.2.0   | 2018-06-22 | Added ARcher the Hunter(developer version) support. |
| 0.1.0   | 2018-06-21 | Added AR Gun the Elite(developer version) support.  |

## Setup Guide

1. Import this package into your Unity project.
2. Drag the "GeekplaySDK" prefab to the very first scene.
3. Edit the items of "GeekplaySDK".
4. Create a script that finds "GeekplaySDK" and calls the APIs you require.

## Example Code

```c#
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
        
        if (DeviceName.ARGUN == sdk.m_deviceName)
        {
            gun = sdk.GetDevice() as GeekplayARGun;
            gun.Initialize(GunShoot);
        }
        else if (DeviceName.ARCHER == sdk.m_deviceName)
        {
            bow = sdk.GetDevice() as GeekplayARcher;
            bow.Initialize(BowDraw, BowShoot, null, null);
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
        Debug.Log("Button: " + bow.GetState().buttonPressed);
    }
}
```

## Support

email: info@geekplay.cc

## API Reference

### GeekplaySDK : MonoBehaviour

```c#
//	The Enum of device name. Do not edit manually. Select it from the Unity editor.
DeviceName m_deviceName;

//	Get current device indicated by "m_deviceName". Don't forget to convert the type explicitly. 
//	Call this method before any other Geekplay APIs.
GeekplayDevice GetDevice();

//	Register the device to Geekplay server (not necessary)
//	When complete(success or failed), _complete callback will be executed.
//	Once you start registering, don't call any other SDK APIs until it's completed.
void RegisterDevice(Action _complete = null);
```

### GeekplayARGun : GeekplayDevice

```c#
//	Initialize AR Gun. When initialized, _complete callback will be executed. 
//	When you pull the trigger, _shootHandler callback will be executed.
void Initialize(Action _shootHandler = null, Action _complete = null);

//	data of AR Gun
//	triggerDown: true ~ trigger down, false ~ trigger up
//	joyStickX:   0.0 ~ middle, 1.0 ~ up, -1.0 ~ down
//	joyStickY:   0.0 ~ middle, 1.0 ~ right, -1.0 ~ left
public class AR_Gun
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

//	Get the data of trigger and joystick
ARGunState GetState();
```

### GeekplayARcher : GeekplayDevice

```c#
//	Initialize ARcher. When initialized, _complete callback will be executed. 
//	When you draw the bow, _drawHandler callback will be executed.
//	When you shoot, _shootHandler callback will be executed.
//	When you press the side button, _pressHandler callback will be executed.
//	When you release the button, _releaseHandler callback will be executed.
void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _pressHandler = null, Action _releaseHandler = null, Action _complete = null);

//	data of ARcher
//	stringPulled:  true ~ string pulled, false ~ string released
//	buttonPressed: true ~ button pressed, false ~ button released
public class ARcher
{
    public bool stringPulled = false;
    public bool buttonPressed = false;
}

//	Get the data of string and button
ARcherState GetState();
```

