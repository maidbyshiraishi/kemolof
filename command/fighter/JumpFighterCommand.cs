using Godot;
using kemolof.mob.fighter;
using kemolof.system.waza_analyzer;

namespace kemolof.command.fighter;

/// <summary>
/// ファイターをジャンプさせるコマンド
/// </summary>
public partial class JumpFighterCommand : CommandRoot
{
    [Export(PropertyHint.Enum, "Disable, Left(Static), Right(Static), Forward, Back")]
    public int Direction { get; set; }

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag || node is not FighterRoot fighter || !fighter.IsOnFloor())
        {
            return;
        }

        int direction = 0;

        switch (Direction)
        {
            case 1:
                direction = WazaKey.Left;
                break;

            case 2:
                direction = WazaKey.Right;
                break;

            case 3:
                direction = fighter.FighterRightSide ? WazaKey.Right : WazaKey.Left;
                break;

            case 4:
                direction = fighter.FighterRightSide ? WazaKey.Left : WazaKey.Right;
                break;
        }

        fighter.ControlOuter([WazaKey.Free, WazaKey.Up, direction]);
    }
}
