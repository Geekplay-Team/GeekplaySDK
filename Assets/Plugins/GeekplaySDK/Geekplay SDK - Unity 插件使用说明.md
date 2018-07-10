# Geekplay SDK - Unity 插件使用说明

## 简介

本插件可以与 Geekplay 的硬件产品连接并通信，实现用户自定义游戏与 Geekplay 产品的结合。

## 版本变更

| 版本  | 日期       | 描述                         |
| ----- | ---------- | ---------------------------- |
| 0.4.0 | 2018-07-10 | 兼容新款 ARcher              |
| 0.3.0 | 2018-07-05 | 新增了合法性验证的功能       |
| 0.2.0 | 2018-06-22 | 兼容 ARcher（开发者版）      |
| 0.1.0 | 2018-06-21 | 兼容 AR Gun 精锐（开发者版） |

## 使用方式

1. 将本插件导入 Unity 工程。
2. 将名为 GeekplaySDK 的预制体拖放到第一个场景。
3. 根据需求，编辑 GeekplaySDK 的相关信息。
4. 创建脚本，调用相关 API。

## 示例代码

```c#
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
```

## 技术支持

email: info@geekplay.cc

## API 说明

### GeekplaySDK : MonoBehaviour

```c#
//	设备名。请从 Unity 编辑器选择，勿手动编辑。
DeviceName m_deviceName;

//	获取当前设备，调用时需要显式类型转换。确保在调用其他 API 之前获取设备。
GeekplayDevice GetDevice();

//	将当前设备注册到 Geekplay 服务器（非必需）。注册成功后，_complete 会被回调。开始注册后，在注册完成之前，请不要调用其他 Geekplay API。
void RegisterDevice(Action _complete = null);
```

### GeekplayARGun : GeekplayDevice

```c#
//	初始化 AR Gun，完成后 _complete 会被回调。
//	扣动扳机时，_shootHandler 会被回调。
void Initialize(Action _shootHandler = null, Action _complete = null);

//	AR Gun 的相关数据
//	triggerDown: true ~ 扳机被按下, false ~ 扳机未按下
//	joyStickX:   0.0 ~ 中央, 1.0 ~ 向上, -1.0 ~ 乡下
//	joyStickY:   0.0 ~ 中央, 1.0 ~ 向右, -1.0 ~ 向左
public class AR_Gun
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

//	获取扳机与摇杆的当前状态
ARGunState GetState();
```

### GeekplayARcher : GeekplayDevice

```c#
//	初始化 ARcher，完成后 _complete 会被回调。
//	拉弓时, _drawHandler 会被回调。
//	射箭时, _shootHandler 会被回调。
//	按下按键时, _pressHandler 会被回调。
//	松开按键时, _releaseHandler 会被回调。
void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _pressHandler = null, Action _releaseHandler = null, Action _complete = null);

//	ARcher 的相关数据
//	stringPulled:  true ~ 弓弦被拉开, false ~ 弓弦被释放
//	buttonPressed: true ~ 按钮被按下, false ~ 按钮被松开
public class ARcher
{
    public bool stringPulled = false;
    public bool buttonPressed = false;
}

//	获取弓弦与按钮的当前状态
ARcherState GetState();
```
