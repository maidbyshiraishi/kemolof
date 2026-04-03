using Godot;
using maid_by_shiraishi.mob.fighter;

namespace maid_by_shiraishi.command.fighter;

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
