using Godot;
using Godot.Collections;
using kemolof.command.fighter;
using kemolof.mob.fighter;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.brain;

/// <summary>
/// ファイターCPU操作の親
/// </summary>
public partial class CpuFighterRoot : BrainRoot, IOuterControledBrain
{
    /// <summary>
    /// ファイターID
    /// </summary>
    [Export]
    public int FighterId { get; set; }

    /// <summary>
    /// 技1発動確率
    /// </summary>
    [Export]
    public float Waza1Probability { get; set; } = 0.1f;

    /// <summary>
    /// 技1発動確率
    /// </summary>
    [Export]
    public float Waza2Probability { get; set; } = 0.1f;

    /// <summary>
    /// 技1発動確率
    /// </summary>
    [Export]
    public float Waza3Probability { get; set; } = 0.1f;

    /// <summary>
    /// 通常行動確率
    /// </summary>
    [Export]
    public float ActionProbability { get; set; } = 0.33333f;

    protected RandomNumberGenerator m_Random = new();
    protected Array<int> m_NextKey = [];
    protected Array<int> m_OuterControlKey = [];
    protected SightArea m_OverheadArea;
    protected SightArea m_AttackArea;
    protected SightArea m_BackArea;
    protected SightArea m_UnderfootArea;
    protected RayCast2D m_JumpRay;
    protected RayCast2D m_GroundRay;

    public override void _Ready()
    {
        base._Ready();
        m_OverheadArea = GetNode<SightArea>("OverheadArea");
        m_AttackArea = GetNode<SightArea>("AttackArea");
        m_BackArea = GetNode<SightArea>("BackArea");
        m_UnderfootArea = GetNode<SightArea>("UnderfootArea");
        m_JumpRay = GetNode<RayCast2D>("JumpRay");
        m_GroundRay = GetNode<RayCast2D>("GroundRay");
        m_Random.Randomize();

        if (GetParent() is FighterRoot fighter)
        {
            _ = GetParent<FighterRoot>();
            FighterId = fighter.FighterId;
        }
    }

    protected void InsertBufferSequence(int key, int frame)
    {
        for (int i = 0; i < frame; i++)
        {
            m_NextKey.Add(key);
        }
    }

    protected virtual void InsertWaza1()
    {
    }

    protected virtual void InsertWaza2()
    {
    }

    protected virtual void InsertWaza3()
    {
    }

    protected virtual void InsertGuard()
    {
        int key = Fighter.RightSide ? WazaKey.Right : WazaKey.Left;
        InsertBuffer(key);
    }

    protected virtual void InsertCrouchGuard()
    {
        int key = WazaKey.Down | (Fighter.RightSide ? WazaKey.Right : WazaKey.Left);
        InsertBuffer(key);
    }

    protected virtual void InsertCrouchKick()
    {
        int key = WazaKey.Down | WazaKey.Kick;
        InsertBuffer(WazaKey.Down);
        InsertBufferSequence(WazaKey.Down, 1);
        InsertBufferSequence(key, 3);
    }

    protected virtual void InsertWalk()
    {
        int key = Fighter.RightSide ? WazaKey.Left : WazaKey.Right;
        InsertBuffer(key);
        InsertBufferSequence(key, 5);
    }

    protected virtual void InsertJumpKick()
    {
        InsertBuffer(WazaKey.Kick | (Fighter.RightSide ? WazaKey.Left : WazaKey.Right));
    }

    #region ファイター外部制御インタフェース
    /// <summary>
    /// ファイターを外部から制御する
    /// </summary>
    /// <param name="command">コマンド列</param>
    public virtual void OuterControl(Array<int> command)
    {
        if (m_OuterControlKey.Count == 0)
        {
            m_NextKey.Clear();
            m_OuterControlKey.AddRange(command);
        }
    }
    #endregion
}
