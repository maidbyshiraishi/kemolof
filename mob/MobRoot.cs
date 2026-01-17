using Godot;

namespace kemolof.mob;

/// <summary>
/// 物理挙動するオブジェクトの親
/// </summary>
public partial class MobRoot : CharacterBody2D
{
    [Export]
    public bool RightSide { get; set; }

    [ExportGroup("Fighter Physical Ability")]

    /// <summary>
    /// 地上での助走距離
    /// </summary>
    [Export]
    public float GroundApproach { get; set; } = 4f;

    /// <summary>
    /// 地上での最大速度
    /// </summary>
    [Export]
    public float GroundMaxSpeed { get; set; } = 200f;

    /// <summary>
    /// 地上での減速距離
    /// </summary>
    [Export]
    public float GroundReductionApproach { get; set; } = 4f;

    /// <summary>
    /// 地上での助走距離
    /// </summary>
    [Export]
    public float GuardApproach { get; set; } = 4f;

    /// <summary>
    /// ガード中での最大速度
    /// </summary>
    [Export]
    public float GuardMaxSpeed { get; set; } = 120f;

    /// <summary>
    /// 地上での減速距離
    /// </summary>
    [Export]
    public float GuardReductionApproach { get; set; } = 4f;

    /// <summary>
    /// 空中での助走距離
    /// </summary>
    [Export]
    public float AirApproach { get; set; } = 64f;

    /// <summary>
    /// 空中での最大速度
    /// </summary>
    [Export]
    public float AirMaxSpeed { get; set; } = 350f;

    /// <summary>
    /// 空中での減速距離
    /// </summary>
    [Export]
    public float AirReductionApproach { get; set; } = 16f;

    /// <summary>
    /// ジャンプの高さ
    /// </summary>
    [Export]
    public float JumpHeight { get; set; } = 128f;

    /// <summary>
    /// ジャンプにかかる時間
    /// </summary>
    [Export]
    public float JumpTime { get; set; } = 0.5f;

    protected float m_GroundAcceleration;
    protected float m_GroundReductionAcceleration;
    protected float m_GuardAcceleration;
    protected float m_GuardReductionAcceleration;
    protected float m_AirAcceleration;
    protected float m_AirReductionAcceleration;
    protected float m_JumpVelocity;
    protected float m_Gravity;
    protected float m_Delta;

    public override void _Ready()
    {
        m_GroundAcceleration = Mathf.Pow(GroundMaxSpeed, 2f) / (GroundApproach * 2f);
        m_GroundReductionAcceleration = Mathf.Pow(GroundMaxSpeed, 2f) / (GroundReductionApproach * 2f);
        m_GuardAcceleration = Mathf.Pow(GuardMaxSpeed, 2f) / (GuardApproach * 2f);
        m_GuardReductionAcceleration = Mathf.Pow(GuardMaxSpeed, 2f) / (GuardReductionApproach * 2f);
        m_AirAcceleration = Mathf.Pow(AirMaxSpeed, 2f) / (AirApproach * 2f);
        m_AirReductionAcceleration = Mathf.Pow(AirMaxSpeed, 2f) / (AirReductionApproach * 2f);
        m_Gravity = 2f * JumpHeight / Mathf.Pow(JumpTime, 2);
        m_JumpVelocity = -Mathf.Sqrt(2f * m_Gravity * JumpHeight);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        m_Delta = (float)delta;
    }

    protected virtual void GravityOnly(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.Y += m_Gravity * (float)delta;
        Velocity = velocity;
    }

    protected virtual void StopWalk(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.X = 0f;
        Velocity = velocity;
    }

    protected virtual void GroundWalkAction(double delta, bool forward, bool back, bool rightSide)
    {
        Vector2 velocity = Velocity;
        bool right = rightSide ? back : forward;
        bool left = rightSide ? forward : back;

        if ((Velocity.X < 0 && right) || (0 < Velocity.X && left))
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, 0, (m_GroundReductionAcceleration + m_GroundAcceleration) * delta);
        }
        else if (left)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, -GroundMaxSpeed, m_GroundAcceleration * delta);
        }
        else if (right)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, GroundMaxSpeed, m_GroundAcceleration * delta);
        }

        Velocity = velocity;
    }

    protected virtual void WalkForwardAction(double delta, bool rightSide)
    {
        Vector2 velocity = Velocity;
        bool right = !rightSide;
        bool left = rightSide;

        if ((Velocity.X < 0 && right) || (0 < Velocity.X && left))
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, 0, (m_GroundReductionAcceleration + m_GroundAcceleration) * delta);
        }
        else if (left)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, -GroundMaxSpeed, m_GroundAcceleration * delta);
        }
        else if (right)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, GroundMaxSpeed, m_GroundAcceleration * delta);
        }

        Velocity = velocity;
    }

    protected virtual void GuardWalkAction(double delta, bool rightSide)
    {
        Vector2 velocity = Velocity;
        bool left = !rightSide;
        velocity.X = (float)Mathf.MoveToward(velocity.X, left ? -GuardMaxSpeed : GuardMaxSpeed, m_GuardAcceleration * delta);
        Velocity = velocity;
    }

    protected virtual void AirWalkAction(double delta, bool forward, bool back, bool rightSide)
    {
        Vector2 velocity = Velocity;
        bool right = rightSide ? back : forward;
        bool left = rightSide ? forward : back;

        if ((Velocity.X < 0 && right) || (0 < Velocity.X && left))
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, 0, (m_AirReductionAcceleration + m_AirAcceleration) * delta);
        }
        else if (left)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, -AirMaxSpeed, m_AirAcceleration * delta);
        }
        else if (right)
        {
            velocity.X = (float)Mathf.MoveToward(velocity.X, AirMaxSpeed, m_AirAcceleration * delta);
        }

        Velocity = velocity;
    }

    protected void ReductionAction(double delta, bool onFloor)
    {
        if (onFloor)
        {
            GroundReductionAction(delta);
            return;
        }

        AirReductionAction(delta);
    }

    protected void GroundReductionAction(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.X = (float)Mathf.MoveToward(velocity.X, 0, m_GroundReductionAcceleration * delta);
        Velocity = velocity;
    }

    protected void AirReductionAction(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.X = (float)Mathf.MoveToward(velocity.X, 0, m_AirReductionAcceleration * delta);
        Velocity = velocity;
    }

    protected virtual void JumpAction(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.Y = m_JumpVelocity;
        Velocity = velocity;
    }
}
