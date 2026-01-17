using Godot;
using Godot.Collections;
using kemolof.mob.brain;
using kemolof.stage;
using kemolof.system;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.fighter;

/// <summary>
/// ファイターの親
/// </summary>
public partial class FighterRoot : MobRoot, IGameNode
{
    /// <summary>
    /// 撃破されたシグナル
    /// </summary>
    /// <param name="fighter">ファイター番号</param>
    [Signal]
    public delegate void DefeatedEventHandler(FighterRoot fighter);

    /// <summary>
    /// ファイター名
    /// </summary>
    [Export]
    public string FighterName { get; set; }

    /// <summary>
    /// ファイター番号
    /// </summary>
    [Export]
    public int FighterId { get; set; }

    /// <summary>
    /// ファイターの方向
    /// </summary>
    [Export]
    public bool FighterRightSide { get; set; }

    /// <summary>
    /// ファイター色
    /// </summary>
    [Export]
    public Color FighterColor { get; set; }

    /// <summary>
    /// 死亡か
    /// </summary>
    [Export]
    public bool Dead { get; set; } = false;

    /// <summary>
    /// リスポーン地点
    /// </summary>
    [Export]
    public Vector2 RespawnPosition { get; set; }

    /// <summary>
    /// GameStageRoot
    /// </summary>
    [Export]
    public GameStageRoot StageRoot { get; set; }

    /// <summary>
    /// ステートマシンの状態
    /// </summary>
    [Export]
    public string State { get; set; } = null;

    [ExportGroup("Hit Effect")]

    [Export]
    public PackedScene EffectDamage { get; set; }

    [Export]
    public PackedScene EffectGuard { get; set; }

    [ExportGroup("Voice")]

    [Export]
    public Array<string> MieVoice { get; set; } = [];

    [Export]
    public Array<string> ActionVoice { get; set; } = [];

    [Export]
    public Array<string> DamageVoice { get; set; } = [];

    [Export]
    public Array<string> KnockDownVoice { get; set; } = [];

    [Export]
    public Array<string> DeadVoice { get; set; } = [];

    [Export]
    public Array<string> Waza1Voice { get; set; } = [];

    [Export]
    public Array<string> Waza2Voice { get; set; } = [];

    [Export]
    public Array<string> Waza3Voice { get; set; } = [];

    [ExportGroup("Se")]

    [Export]
    public Array<string> JumpSe { get; set; } = ["fighter_jump"];

    [Export]
    public Array<string> JumpKickSe { get; set; } = ["fighter_kick"];

    protected AnimationTree m_AnimationTree;
    protected AnimationNodeStateMachinePlayback m_StateMachine;
    protected BrainRoot m_Brain;
    protected TextureProgressBar m_LifeBar;
    protected SePlayer m_SePlayer;

    private WazaAnalyzer _wazaAnalyzer;
    private Node2D _character;
    private FighterList _fighterList;
    private FighterRoot _targetFighter;
    private Array<AttackArea> _attackArea = [];

    public override void _Ready()
    {
        base._Ready();

        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<DamageArea>("Character/DamageArea1")?.Connect(DamageArea.SignalName.Damaged, new(this, MethodName.Damaged));
        _ = GetNodeOrNull<DamageArea>("Character/DamageArea2")?.Connect(DamageArea.SignalName.Damaged, new(this, MethodName.Damaged));
        _ = GetNodeOrNull<DamageArea>("Character/DamageArea3")?.Connect(DamageArea.SignalName.Damaged, new(this, MethodName.Damaged));

        m_Brain = GetNode<BrainRoot>("Brain");
        m_SePlayer = GetNode<SePlayer>("SePlayer");
        m_AnimationTree = GetNode<AnimationTree>("AnimationTree");
        m_StateMachine = (AnimationNodeStateMachinePlayback)m_AnimationTree.Get("parameters/playback");
        m_LifeBar = GetNode<TextureProgressBar>("Status/LifeBar");
        m_LifeBar.TintProgress = FighterColor;
        Label label = GetNode<Label>("Status/No");
        label.Modulate = FighterColor;
        label.Text = $"No.{FighterId + 1}";
        _character = GetNode<Node2D>("Character");
        _fighterList = GetParent<FighterList>();
        _wazaAnalyzer = GetNode<WazaAnalyzer>("WazaAnalyzer");
        EntryWaza();

        foreach (Node child in GetNode("Character").GetChildren())
        {
            if (child is AttackArea attackArea)
            {
                attackArea.FighterId = FighterId;
                _attackArea.Add(attackArea);
            }
            else if (child is DamageArea damageArea)
            {
                damageArea.FighterId = FighterId;
            }
        }

        AddToGroup(IGameNode.GameNodeGroup);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (m_Brain is null || _wazaAnalyzer is null)
        {
            return;
        }

        State = m_StateMachine.GetCurrentNode();

        if (State is "mie")
        {
            return;
        }

        _targetFighter = _fighterList.GetNeighborFighter(this);
        UpdatePositioning();
        UpdateDirection(delta);
        UpdateFighter(delta);
    }

    protected virtual void EntryWaza()
    {
    }

    public void SetStartDirection()
    {
        _targetFighter = _fighterList.GetNeighborFighter(this);

        if (GlobalPosition.X < _targetFighter.GlobalPosition.X)
        {
            RightSide = false;
            SetFighterDirection(RightSide);
        }
        else if (_targetFighter.GlobalPosition.X < GlobalPosition.X)
        {
            RightSide = true;
            SetFighterDirection(RightSide);
        }
    }

    private void UpdatePositioning()
    {
        if (_targetFighter is null)
        {
            return;
        }

        if (GlobalPosition.X < _targetFighter.GlobalPosition.X)
        {
            RightSide = false;
        }
        else if (_targetFighter.GlobalPosition.X < GlobalPosition.X)
        {
            RightSide = true;
        }
    }

    protected virtual void UpdateDirection(double delta)
    {
        string state = m_StateMachine.GetCurrentNode();

        if (state is "stand" && FighterRightSide != RightSide)
        {
            m_Brain.ClearBuffer();
            SetFighterDirection(RightSide);
        }
    }

    protected virtual void UpdateFighter(double delta)
    {
    }

    protected void AddWaza(string wazaName, Array<int> wazaCommand, Array<int> wazaMask)
    {
        _wazaAnalyzer.AddWaza(wazaName, wazaCommand, wazaMask);
    }

    protected bool AnalyzeWaza(string wazaName, Array<int> buffer)
    {
        if (_wazaAnalyzer.AnalyzeWaza(wazaName, buffer, FighterRightSide))
        {
            m_Brain.ClearBuffer();
            return true;
        }

        return false;
    }

    public virtual void SetActionVelocity(Vector2 velocity)
    {
        if (FighterRightSide)
        {
            velocity.X *= -1f;
        }

        Velocity = velocity;
    }

    protected virtual void CrouchAction(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.X = 0;
        Velocity = velocity;
    }

    private void SetFighterDirection(bool rightSide)
    {
        _character.Scale = new Vector2(rightSide ? -1f : 1f, 1f);
        FighterRightSide = rightSide;

        foreach (AttackArea attackeArea in _attackArea)
        {
            attackeArea.RightSide = FighterRightSide;
        }
    }

    protected void KnockBack(bool rightSide, float knockBackVelocity)
    {
        Vector2 velocity = Velocity;
        velocity.X = (rightSide ? -1f : 1f) * knockBackVelocity;
        Velocity = velocity;
    }

    public virtual void Outside(bool moveSpawnPosition, int damage)
    {
        if (moveSpawnPosition)
        {
            GlobalPosition = RespawnPosition;
        }

        double life = m_LifeBar.Value;

        if (Dead || damage == 0)
        {
            return;
        }

        life -= damage;

        if (life <= 0)
        {
            m_StateMachine.Start("dead");
            m_LifeBar.Value = 0;
            Dead = true;
            SetCollisionLayerValue(4, false);
            _ = EmitSignal(SignalName.Defeated, [this]);
            return;
        }

        m_LifeBar.Value = life;
    }

    public virtual void Damaged(AttackArea attackArea, DamageArea damageArea, int row)
    {
        GD.Print("attacked!");

        double life = m_LifeBar.Value;

        if (Dead || attackArea.Attack == 0)
        {
            GD.Print("attack zero");
            return;
        }

        life -= attackArea.Attack;

        if (life <= 0)
        {
            m_StateMachine.Start("dead");
            m_LifeBar.Value = 0;
            Dead = true;
            SetCollisionLayerValue(4, false);
            _ = EmitSignal(SignalName.Defeated, [this]);
            return;
        }

        AddDamageEffect(EffectDamage, damageArea);
        m_StateMachine.Start("damaged");
        m_LifeBar.Value = life;
    }

    protected void AddDamageEffect(PackedScene effect, DamageArea damageArea)
    {
        if (effect is not null && effect.Instantiate() is Node2D node2d)
        {
            RandomNumberGenerator random = new();
            node2d.Position += new Vector2(random.RandfRange(-16f, 16f), random.RandfRange(-16f, 16f));
            damageArea.AddChild(node2d);
        }
    }

    public virtual void ControlOuter(Array<int> command)
    {
    }

    /// <summary>
    /// ステートマシンを着地モードへ遷移する
    /// </summary>
    /// <param name="force">強制的に遷移するか</param>
    public virtual void SetStateLanding(bool force)
    {
    }

    public void PlayMieVoice()
    {
        PlayRandomVoice(MieVoice);
    }

    public void PlayActionVoice()
    {
        PlayRandomVoice(ActionVoice);
    }

    public void PlayDamageVoice()
    {
        PlayRandomVoice(DamageVoice);
    }

    public void PlayKnockDownVoice()
    {
        PlayRandomVoice(KnockDownVoice);
    }

    public void PlayDeadVoice()
    {
        PlayRandomVoice(DeadVoice);
    }

    protected void PlayRandomVoice(Array<string> voice)
    {
        if (voice.Count != 0)
        {
            m_SePlayer.Play(voice.PickRandom(), true);
        }
    }

    protected void PlayVoice(string voice)
    {
        if (!string.IsNullOrWhiteSpace(voice))
        {
            m_SePlayer.Play(voice, true);
        }
    }

    /// <summary>
    /// 左右キー入力をファイター側の前方・後方として取得する
    /// </summary>
    /// <param name="rightSide">右側か</param>
    /// <param name="forward">前方入力</param>
    /// <param name="back">後方入力</param>
    internal void IsForwardOrBack(bool rightSide, out bool forward, out bool back)
    {
        bool right = m_Brain.IsKeyPressed(WazaKey.Right, WazaKey.Right);
        bool left = m_Brain.IsKeyPressed(WazaKey.Left, WazaKey.Left);
        forward = rightSide ? left : right;
        back = rightSide ? right : left;
    }

    #region IGameNodeインタフェース
    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void InitializeNode()
    {
        m_StateMachine.Start("mie");
    }
    #endregion
}
