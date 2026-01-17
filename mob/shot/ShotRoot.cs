using Godot;
using Godot.Collections;

namespace kemolof.mob.shot;

/// <summary>
/// 飛び道具の親
/// </summary>
public partial class ShotRoot : Area2D, IShot
{
    [Export]
    public Vector2 Velocity { get; set; }

    [Export]
    public bool RightSide { get; set; }

    [Export]
    public int FighterId { get; set; }

    [Export]
    public bool Disable { get; set; } = false;

    private Array<AttackArea> _attackArea = [];
    private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;

    public override void _Ready()
    {
        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<Area2D>("EraseCollision")?.Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.HitArea2D));
        _ = GetNodeOrNull<Area2D>("EraseCollision")?.Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.HitNode2D));

        Array<Node> children = GetChildren();
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = RightSide;

        foreach (Node child in children)
        {
            if (child is AttackArea attackArea)
            {
                attackArea.FighterId = FighterId;
                attackArea.RightSide = RightSide;
                _attackArea.Add(attackArea);
            }
        }

        _visibleOnScreenNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.X *= RightSide ? -1f : 1f;
        GlobalPosition += velocity * (float)delta;

        if (!_visibleOnScreenNotifier.Visible)
        {
            QueueFree();
        }
    }

    public void HitArea2D(Area2D area)
    {
        if (area is DamageArea darea && darea.FighterId == FighterId)
        {
            return;
        }

        QueueFree();
    }

    public void HitNode2D(Node2D node)
    {
        if (node is DamageArea darea && darea.FighterId == FighterId)
        {
            return;
        }

        QueueFree();
    }

    #region IShotインタフェース
    public void Initialize(Vector2 globalPosition, int fighterId, bool rightSide)
    {
        GlobalPosition = globalPosition;
        FighterId = fighterId;
        RightSide = rightSide;
    }
    #endregion
}
