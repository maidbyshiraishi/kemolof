using Godot;

namespace kemolof.command;

/// <summary>
/// CollisionShape2DのDisableを切り替えるコマンド
/// </summary>
public partial class DisableCollisionShapeCommand : CommandRoot
{
    [Export]
    public Node Target { get; set; }

    /// <summary>
    /// フラグ名
    /// </summary>
    [Export]
    public bool Value { get; set; } = false;

    public override void ExecCommand(Node node, bool flag)
    {
        if (ExecFlag != flag)
        {
            return;
        }

        if (Target is not null)
        {
            node = Target;
        }

        if (node is CollisionShape2D collisionShape2D)
        {
            collisionShape2D.Disabled = Value;
        }
    }
}
