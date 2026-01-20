using kemolof.system.joy_pad_controller;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.brain;

/// <summary>
/// ファイターゲームパッド操作
/// </summary>
public partial class HumanPad : BrainRoot
{
    private JoyPadController _pad;

    public override void _Ready()
    {
        base._Ready();
        _pad = GetNode<JoyPadController>("/root/JoyPadController");
    }

    public override void _Process(double delta)
    {
        if (Fighter.State is "mie")
        {
            return;
        }

        // フレームをまたいで押下状態を確認しなければならないため
        // 押したフレームにだけ反応するIsActionJustPressedではダメ
        InsertBuffer(
              (_pad.IsActionPressed(DeviceIndex, "joy_pad_up") ? WazaKey.Up : 0)
            | (_pad.IsActionPressed(DeviceIndex, "joy_pad_down") ? WazaKey.Down : 0)
            | (_pad.IsActionPressed(DeviceIndex, "joy_pad_left") ? WazaKey.Left : 0)
            | (_pad.IsActionPressed(DeviceIndex, "joy_pad_right") ? WazaKey.Right : 0)
            | (_pad.IsActionPressed(DeviceIndex, "joy_pad_punch") ? WazaKey.Punch : 0)
            | (_pad.IsActionPressed(DeviceIndex, "joy_pad_kick") ? WazaKey.Kick : 0)
        );
    }
}
