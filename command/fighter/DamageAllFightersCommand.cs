using Godot;
using Godot.Collections;
using kemolof.stage;
using kemolof.system;

namespace kemolof.command.fighter;

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

        GameStageRoot gameStageRoot = GetNode<GameDialogLayer>("/root/DialogLayer").GetCurrentGameRoot();
        gameStageRoot.DamageAllFighters(Damage, HitVoice);
    }
}
