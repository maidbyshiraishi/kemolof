using Godot;
using Godot.Collections;
using maid_by_shiraishi.stage;
using maid_by_shiraishi.system;

namespace maid_by_shiraishi.command.fighter;

/// <summary>
/// ファイター全員にダメージコマンド
/// </summary>
public partial class DamageAllFightersCommand : CommandRoot
{
    [Export]
    public int Damage { get; set; } = 5;

    [Export]
    public Array<string> HitVoice { get; set; } = [];

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag)
        {
            return;
        }

        GameStageRoot gameStageRoot = GetNode<DialogLayer>("/root/DialogLayer").GetCurrentGameStageRoot();
        gameStageRoot.DamageAllFighters(Damage, HitVoice);
    }
}
