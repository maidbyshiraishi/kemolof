using Godot;
using Godot.Collections;
using kemolof.system;

namespace kemolof.mob.shot;

/// <summary>
/// 忍者必殺技3
/// </summary>
public partial class Fighter4Shot2 : MobRoot, IShot
{
    [Export]
    public int FighterId { get; set; }

    [Export]
    public int JumpLimit { get; set; } = 3;

    protected SePlayer m_SePlayer;

    private Array<AttackArea> _attackArea = [];
    private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;
    private bool _wakeupd = false;
    private AnimatedSprite2D _animatedSprite2D;
    private AnimatedSprite2D _effect;

    public override void _Ready()
    {
        base._Ready();

        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<AnimatedSprite2D>("Effect")?.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new(this, MethodName.AnimationFinished));

        m_SePlayer = GetNode<SePlayer>("SePlayer");
        _effect = GetNode<AnimatedSprite2D>("Effect");
        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite2D.Visible = false;
        _animatedSprite2D.FlipH = RightSide;
        Array<Node> children = GetChildren();

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
        _visibleOnScreenNotifier.Show();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_visibleOnScreenNotifier.Visible)
        {
            QueueFree();
            return;
        }

        if (!_wakeupd || JumpLimit < 0)
        {
            return;
        }

        if (JumpLimit == 0)
        {
            JumpLimit--;
            _effect.Play();
            return;
        }

        if (IsOnFloor())
        {
            m_JumpVelocity *= 0.85f;
            JumpLimit--;
            m_SePlayer.Play("fighter_4_shot_2_jump");
            JumpAction(delta);
            _ = MoveAndSlide();
            return;
        }

        GravityOnly(delta);
        AirWalkAction(delta, true, false, RightSide);
        _ = MoveAndSlide();
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

    public void AnimationFinished()
    {
        m_SePlayer.Play("fighter_4_shot_2_effect");

        // 規定回数ジャンプした後、アニメ終了後消滅する
        if (_wakeupd)
        {
            QueueFree();
        }
        // 出現時のアニメが終了したら動作開始する
        else
        {
            _wakeupd = true;
            _animatedSprite2D.Visible = true;
            GetNode<CollisionShape2D>("AttackArea1/AttackCollision").Disabled = false;
        }
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
