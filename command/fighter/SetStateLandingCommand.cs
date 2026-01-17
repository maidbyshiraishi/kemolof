using Godot;
using kemolof.mob.fighter;

namespace kemolof.command.fighter;

/// <summary>
/// ファイターを着陸状態へ遷移させるコマンド
/// </summary>
public partial class SetStateLandingCommand : CommandRoot
{
    [Export]
    public bool Force { get; set; } = false;

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag || node is not FighterRoot fighter)
        {
            return;
        }

        fighter.SetStateLanding(Force);
    }
}
