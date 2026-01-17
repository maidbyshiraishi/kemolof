using Godot;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.brain;

/// <summary>
/// ファイターキーボード操作
/// </summary>
public partial class HumanKeyb : BrainRoot
{
    public override void _Process(double delta)
    {
        if (Fighter.State is "mie")
        {
            return;
        }

        // フレームをまたいで押下状態を確認しなければならないため
        // 押したフレームにだけ反応するIsActionJustPressedではダメ
        InsertBuffer(
              (Input.IsActionPressed($"game_keyb{DeviceIndex}_up") ? WazaKey.Up : 0)
            | (Input.IsActionPressed($"game_keyb{DeviceIndex}_down") ? WazaKey.Down : 0)
            | (Input.IsActionPressed($"game_keyb{DeviceIndex}_left") ? WazaKey.Left : 0)
            | (Input.IsActionPressed($"game_keyb{DeviceIndex}_right") ? WazaKey.Right : 0)
            | (Input.IsActionPressed($"game_keyb{DeviceIndex}_punch") ? WazaKey.Punch : 0)
            | (Input.IsActionPressed($"game_keyb{DeviceIndex}_kick") ? WazaKey.Kick : 0)
        );
    }
}
