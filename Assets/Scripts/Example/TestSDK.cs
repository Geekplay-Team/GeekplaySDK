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
