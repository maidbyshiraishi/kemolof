using Godot;
using kemolof.mob.fighter;
using kemolof.system;
using kemolof.system.joy_pad_controller;

namespace kemolof.screen;

/// <summary>
/// コントローラー選択画面
/// </summary>
public partial class SelectControllerScreen : DialogRoot
{
    private JoyPadController _joyPad;
    private readonly Mutex _mutex = new();
    private GameDataManager _gameDataManager;

    public override void _Ready()
    {
        base._Ready();
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        _joyPad = GetNode<JoyPadController>("/root/JoyPadController");
        _ = _joyPad.Connect(JoyPadController.SignalName.JoyConnectionChanged, new(this, MethodName.OnJoyConnectionChanged));

        // パッドの接続台数に応じてPad選択ボタンを有効化する。
        int deviceCount = _joyPad.DeviceCount();

        for (int i = 0; i < deviceCount; i++)
        {
            SetJoyButtonEnable(i, true);
        }

        // パッドの接続台数に応じて初期の選択状態を変更する。
        // ノード名は"デバイス_デバイスの番号_プレーヤー番号"
        switch (deviceCount)
        {
            case 0:
                PushButton("Control/Keyb1_1");
                PushButton("Control/Cpu2_2");
                break;

            case 1:
                PushButton("Control/Pad1_1");
                PushButton("Control/Cpu2_2");
                break;

            case >= 2:
                PushButton("Control/Pad1_1");
                PushButton("Control/Pad2_2");
                break;
        }

        PushButton("Control/Uninhabited3");
        PushButton("Control/Uninhabited4");
        PushButton("Control/Uninhabited5");
        PushButton("Control/Uninhabited6");
        PushButton("Control/Uninhabited7");
    }

    public override void Active()
    {
        base.Active();
        int deviceCount = _joyPad.DeviceCount();

        for (int i = 0; i < 7; i++)
        {
            FighterInfo fighterInfo = _gameDataManager.GetFighterInfo(i);

            if (fighterInfo.Uninhabited)
            {
                PushButton($"Control/Uninhabited{i + 1}", false);
            }
            else if (-1 < fighterInfo.Keyb)
            {
                PushButton($"Control/Keyb{fighterInfo.Keyb + 1}_{i + 1}", false);
            }
            else if (-1 < fighterInfo.Pad)
            {
                if (fighterInfo.Pad < deviceCount)
                {
                    PushButton($"Control/Pad{fighterInfo.Pad + 1}_{i + 1}", false);
                }
                else
                {
                    AssignToKeyb(fighterInfo.Pad, i);
                }
            }
            else if (-1 < fighterInfo.Cpu)
            {
                PushButton($"Control/Cpu{fighterInfo.Cpu + 1}_{i + 1}", false);
            }
        }
    }

    public override void Inactive()
    {
        base.Inactive();

        for (int i = 0; i < 7; i++)
        {
            FighterInfo fighterInfo = new();
            ButtonGroup buttonGroup = GetNode<Button>($"Control/Uninhabited{i + 1}").ButtonGroup;
            BaseButton preddedButton = buttonGroup.GetPressedButton();
            string buttonName = preddedButton.Name;

            // 見るからにバカifだけど、別にいいでしょ。
            if (buttonName == $"Keyb1_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Keyb1").Modulate;
                fighterInfo.Keyb = 0;
            }
            else if (buttonName == $"Keyb2_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Keyb2").Modulate;
                fighterInfo.Keyb = 1;
            }
            else if (buttonName == $"Keyb3_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Keyb3").Modulate;
                fighterInfo.Keyb = 2;
            }
            else if (buttonName == $"Pad1_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Pad1").Modulate;
                fighterInfo.Pad = 0;
            }
            else if (buttonName == $"Pad2_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Pad2").Modulate;
                fighterInfo.Pad = 1;
            }
            else if (buttonName == $"Pad3_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Pad3").Modulate;
                fighterInfo.Pad = 2;
            }
            else if (buttonName == $"Pad4_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Pad4").Modulate;
                fighterInfo.Pad = 3;
            }
            else if (buttonName == $"Cpu1_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Cpu1").Modulate;
                fighterInfo.Cpu = 0;
            }
            else if (buttonName == $"Cpu2_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Cpu2").Modulate;
                fighterInfo.Cpu = 1;
            }
            else if (buttonName == $"Cpu3_{i + 1}")
            {
                fighterInfo.Color = GetNode<Label>("Cpu3").Modulate;
                fighterInfo.Cpu = 2;
            }
            else
            {
                fighterInfo.Uninhabited = true;
            }

            _gameDataManager.SetFighterInfo(i, fighterInfo);
        }
    }

    private void AssignToKeyb(int device, int index)
    {
        if (index + 1 == 4)
        {
            PushButton($"Control/Uninhabited{index + 1}", false);
        }
        else
        {
            PushButton($"Control/Keyb{device + 1}_{index + 1}", false);
        }
    }

    private void PushButton(string name, bool noSignal = true)
    {
        if (noSignal)
        {
            GetNode<Button>(name).SetPressedNoSignal(true);
        }
        else
        {
            GetNode<Button>(name).ButtonPressed = true;
        }
    }

    public void OnJoyConnectionChanged(int device, bool connected)
    {
        _mutex.Lock();
        SetJoyButtonEnable(device, connected);

        // 切り離された場合、選択されていたかどうか確認し、
        // キーボードに割り当てる。デバイス4はCPU普通へ割り当てる
        if (!connected)
        {
            _ = _joyPad.DeviceCount();

            for (int i = 0; i < 7; i++)
            {
                if (GetNode<Button>($"Control/Pad{device + 1}_{i + 1}").ButtonPressed)
                {
                    AssignToKeyb(device, i);
                }
            }
        }

        _mutex.Unlock();
    }

    private void SetJoyButtonEnable(int device, bool enabled)
    {
        for (int i = 0; i < 7; i++)
        {
            GetNode<Button>($"Control/Pad{device + 1}_{i + 1}").Disabled = !enabled;
        }
    }
}
