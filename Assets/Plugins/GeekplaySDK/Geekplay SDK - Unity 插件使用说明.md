# Geekplay SDK - Unity 插件使用说明

## 简介 

本插件可以与 Geekplay 的硬件产品连接并通信，实现用户自定义游戏与 Geekplay 产品的结合。

## 版本变更

| 版本  | 日期       | 描述                             |
| ----- | ---------- | -------------------------------- |
| 0.6.0 | 2018-07-26 | 兼容 AR Gun Poseidon（开发者版） |
| 0.5.0 | 2018-07-20 | 支持同时扫描多种设备             |
| 0.4.0 | 2018-07-10 | 兼容新款 ARcher                  |
| 0.3.0 | 2018-07-05 | 新增了合法性验证的功能           |
| 0.2.0 | 2018-06-22 | 兼容 ARcher（开发者版）          |
| 0.1.0 | 2018-06-21 | 兼容 AR Gun 精锐（开发者版）     |

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
    GeekplayDevice m_device;

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
            ((GeekplayPoseidon)_device).Initialize(GunShoot, GunPump);
        }
        else if (typeof(GeekplayHunter) == _device.GetType())
        {
            ((GeekplayHunter)_device).Initialize(BowDraw, BowShoot, ButtonPressed, ButtonReleased);
        }
        else if (typeof(GeekplayDragonbone) == _device.GetType())
        {
            ((GeekplayDragonbone)_device).Initialize(BowDraw, BowShoot); 
        }

        m_device = _device;
    }

    void GunShoot()
    {
        Debug.Log("Gun Shoot: " + Time.time);

        if (typeof(GeekplayPoseidon) == m_device.GetType())
        {
            GeekplayPoseidon gun = (GeekplayPoseidon)m_device;
            gun.MotorRun(0.3f);
            gun.IR_SendMsg(0xCC);
            Debug.Log("X: " + gun.GetState().joyStickX);
            Debug.Log("Y: " + gun.GetState().joyStickY);
        }
    }

    void GunPump()
    {
        Debug.Log("Gun Pump: " + Time.time);
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
```

## 技术支持

email: support@geekplay.cc

## API 说明

### GeekplaySDK : MonoBehaviour

```c#
//	设备名。请从 Unity 编辑器选择，勿手动编辑。
DeviceName m_deviceName;

//	初始化完成的委托定义
delegate void RegisterCallback(GeekplayDevice _device);

//	开始运行 SDK。完成时，_register 将会被回调。开发者可以在 _register 内初始化设备和注册设备相关的回调函数。
void StartSDK(RegisterCallback _register);
```

### GeekplayElite : GeekplayDevice

```c#
//	初始化 AR Gun，完成后 _complete 会被回调。
//	扣动扳机时，_shootHandler 会被回调。
void Initialize(Action _shootHandler = null, Action _complete = null);

//	AR Gun 精锐的相关数据
//	triggerDown: true ~ 扳机被按下, false ~ 扳机未按下
//	joyStickX:   0.0 ~ 中央, 1.0 ~ 向上, -1.0 ~ 乡下
//	joyStickY:   0.0 ~ 中央, 1.0 ~ 向右, -1.0 ~ 向左
class EliteState
{
    public bool triggerDown = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

//	获取扳机与摇杆的当前状态
EliteState GetState();
```

### GeekplayHunter : GeekplayDevice

```c#
//	初始化 ARcher（狩猎者），完成后 _complete 会被回调。
//	拉弓时, _drawHandler 会被回调。
//	射箭时, _shootHandler 会被回调。
//	按下按键时, _pressHandler 会被回调。
//	松开按键时, _releaseHandler 会被回调。
void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _pressHandler = null, Action _releaseHandler = null, Action _complete = null);

//	ARcher（狩猎者）的相关数据
//	stringPulled:  true ~ 弓弦被拉开, false ~ 弓弦被释放
//	buttonPressed: true ~ 按钮被按下, false ~ 按钮被松开
class HunterState
{
    public bool stringPulled = false;
    public bool buttonPressed = false;
}

//	获取弓弦与按钮的当前状态
HunterState GetState();
```

### GeekplayDragonbone : GeekplayDevice

```c#
//	初始化 ARcher（龙骨），完成后 _complete 会被回调。
//	拉弓时, _drawHandler 会被回调。
//	射箭时, _shootHandler 会被回调。
void Initialize(Action _drawHandler = null, Action _shootHandler = null, Action _complete = null);

//	ARcher（龙骨）的相关数据
//	stringPulled:   true ~ 弓弦被拉开, false ~ 弓弦被释放
//	drawLength:     弓弦被拉开的距离
//	fullDrawLength: 普通成年人的满幅拉距
class DragonboneState
{
    public bool stringPulled = false;
    public int drawLength = 0;
    public const float fullDrawLength = 75.0f;
}

//	获取弓的当前状态
DragonboneState GetState();
```

### GeekplayPoseidon : GeekplayDevice

```c#
//	初始化 AR Gun 波塞冬，完成后 _complete 会被回调。
//	扣动扳机时，_shootHandler 会被回调。
//	泵动（装填子弹）时，_pumpHandler 会被回调。
void Initialize(Action _shootHandler = null, Action _pumpHandler = null, Action _complete = null);

//	在初始化之后，如果需要修改回调的注册，可用本方法
void RegisterCallback(Action _shootHandler, Action _pumpHandler)；

//	AR Gun（波塞冬）的相关数据
//	triggerDown: true ~ 扳机被按下, false ~ 扳机未按下
//	pumped:		true ~ 向后拉的状态, false ~ 默认状态
//	joyStickX:   0.0 ~ 中央, 1.0 ~ 向上, -1.0 ~ 乡下
//	joyStickY:   0.0 ~ 中央, 1.0 ~ 向右, -1.0 ~ 向左
public class PoseidonState
{
    public bool triggerDown = false;
    public bool pumped = false;
    public float joyStickX = 0.0f;
    public float joyStickY = 0.0f;
}

//	获取枪的当前状态
PoseidonState GetState();

//	马达振动
//	_time:	振动时间（0-25 秒，精度 0.1 秒）
void MotorRun(float _time);

//	红外发送数据
//	_msg:	需要发送的数据
void IR_SendMsg(byte _msg);
```

