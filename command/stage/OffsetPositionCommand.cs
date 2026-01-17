using Godot;

namespace kemolof.command.stage;

/// <summary>
/// 位置調整を行うコマンド
/// </summary>
public partial class OffsetPositionCommand : CommandRoot
{
    /// <summary>
    /// フラグ名
    /// </summary>
    [Export]
    public Vector2 Value { get; set; } = new();

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag)
        {
            return;
        }

        if (node is Node2D node2d)
        {
            node2d.GlobalPosition += Value;
        }
    }
}
