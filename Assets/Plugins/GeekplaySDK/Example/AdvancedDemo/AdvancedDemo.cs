using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedDemo : MonoBehaviour
{
    GeekplaySDK sdk = null;

    GeekplayNewARcher newBow = null;
    GeekplayARcher bow = null;
    GeekplayARGun gun = null;

    public GameObject drawPoint;
    Vector3 drawOrigin;

    void Start()
    {
        sdk = GameObject.Find("GeekplaySDK").GetComponent<GeekplaySDK>();

        //switch (sdk.m_supportedDevices[0])
        //{
        //    case DeviceName.AR_Gun:
        //        gun = sdk.GetDevice() as GeekplayARGun;
        //        gun.Initialize(GunShoot);
        //        break;
        //    case DeviceName.ARcher:
        //        bow = sdk.GetDevice() as GeekplayARcher;
        //        bow.Initialize(BowDraw, BowShoot, null, null);
        //        break;
        //    case DeviceName.New_ARcher:
        //        newBow = sdk.GetDevice() as GeekplayNewARcher;
        //        newBow.Initialize(BowDraw, BowShoot);
        //        //  记录拉弦点的初始位置
        //        drawOrigin = drawPoint.transform.localPosition;
        //        break;
        //    default:
        //        break;
        //}
    }

    void Update()
    {
        switch (sdk.m_supportedDevices[0])
        {
            case DeviceName.AR_Gun:
                break;
            case DeviceName.ARcher:
                break;
            case DeviceName.New_ARcher:
                float drawLength = newBow.GetState().drawLength / NewARcherState.fullDrawLength;
                drawPoint.transform.localPosition = drawOrigin + new Vector3(drawLength, 0, 0);
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
