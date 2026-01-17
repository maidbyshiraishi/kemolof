using Godot;
using kemolof.mob.fighter;

namespace kemolof.command.fighter;

/// <summary>
/// ファイターが場外
/// </summary>
public partial class OutsideFighterCommand : CommandRoot
{
    [Export]
    public bool MoveSpawnPosition { get; set; } = true;

    [Export]
    public int Damage { get; set; } = 2;

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag || node is not FighterRoot fighter)
        {
            return;
        }

        fighter.Outside(MoveSpawnPosition, Damage);
    }
}
