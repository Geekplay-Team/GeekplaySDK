# Geekplay SDK for Unity

## Introduction

This plugin provides basic access to Geekplay AR Gun, ARcher and AR Unit. You can use this SDK from Unity to connect Geekplay products with your own apps.

## Version Changes

| Version | Description                               |
| ------- | ----------------------------------------- |
| V0.1.0  | Added AR Gun (developer version) support. |

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

    void Start()
    {
        StartSDK();
    }

    void StartSDK()
    {
        sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();
        sdk.StartSDK(Shoot);
    }

    void Shoot()
    {
        Debug.Log("Shoot: " + Time.time);
        Debug.Log("X: " + sdk.GetGunState().joyStickX);
        Debug.Log("Y: " + sdk.GetGunState().joyStickY);
    }
}
```

## Support

email: info@geekplay.cc

## API Reference

```c#
//	Register the device to Geekplay server
//	When complete(success or failed), _complete callback will be executed.
//	Once you start registering, don't call any other SDK APIs until it's completed.
void RegisterDevice(Action _complete = null);

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

//	Start Geekplay SDK. When initialized, _complete callback will be executed. When you pull the trigger, _shootHandler callback will be executed.
void StartSDK(Action _shootHandler, Action _complete = null);

//	Get the data of trigger and joystick
AR_Gun GetGunState();
```



