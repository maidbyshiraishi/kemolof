using Godot;
using Godot.Collections;
using kemolof.command.fighter;
using kemolof.mob.shot;
using kemolof.system;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.fighter;

/// <summary>
/// ファイター4「忍者」
/// </summary>
public partial class Fighter4 : FighterRoot
{
    [Export]
    public float KnockBackVelocity { get; set; } = 280f;

    [Export]
    public float KnockDownVelocity { get; set; } = 800f;

    [Export]
    public float DeadVelocity { get; set; } = 600f;

    [Export]
    public bool Waza2Enabled { get; set; } = false;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Node2D>("Character/Sprites").Modulate = new(0, 0, 0, 0);
    }

    protected override void EntryWaza()
    {
        // waza_1: メイドにあげた手裏剣、いつでも発動
        Array<int> waza1Command =
        [
            WazaKey.Down,
            WazaKey.Right,
            WazaKey.Punch,
        ];
        Array<int> waza1Mask =
        [
            // 青メイドwaza1と共通
            // 下
            WazaKey.Down,
            // 右、左右を間違うな、パンチを同時入力するな、下入れっぱなしは許す
            WazaKey.Left | WazaKey.Right | WazaKey.Punch,
            // パンチ、パンチとキックを間違うな、いつまでも下を押すな、右入れっぱなしは許す
            WazaKey.Down | WazaKey.Punch | WazaKey.Kick,
        ];
        AddWaza("waza_1", waza1Command, waza1Mask);

        // waza_2: 完全シノビの術、地上で発動
        Array<int> waza2Command =
        [
            WazaKey.Down,
            WazaKey.Left,
            WazaKey.Kick,
            WazaKey.Right,
            WazaKey.Down,
            WazaKey.Punch,
        ];
        Array<int> waza2Mask =
        [
            // 下
            WazaKey.Down,
            // 左、キックは同時に押すな、左右は間違うな、下押しっぱなしは許す
            WazaKey.Left | WazaKey.Right | WazaKey.Kick,
            // キック、パンチを同時に押すな、いつまでも下を押すな
            WazaKey.Punch | WazaKey.Kick | WazaKey.Down,
            // 右、左右は間違うな、キック押しっぱなしは許す
            WazaKey.Left | WazaKey.Right,
            // 下、パンチを同時に押すな、いつまでもキックを押すな、右押しっぱなしは許す
            WazaKey.Down | WazaKey.Punch | WazaKey.Kick,
            // パンチ、パンチとキックは間違うな、いつまでも右を押すな、下押しっぱなしは許す
            WazaKey.Right | WazaKey.Punch | WazaKey.Kick,
        ];
        AddWaza("waza_2", waza2Command, waza2Mask);

        // waza_3: ときどき食用、地上で発動
        Array<int> waza3Command =
        [
            WazaKey.Punch,
            WazaKey.Down,
            WazaKey.Right,
            WazaKey.Kick,
            WazaKey.Punch,
            WazaKey.Up,
        ];
        Array<int> waza3Mask =
        [
            // パンチ、パンチとキックを間違うな
            WazaKey.Punch | WazaKey.Kick,
            // 下、パンチ押しっぱなしは許す
            WazaKey.Down,
            // 右、左右は間違うな、キックを同時に押すな、下押しっぱなしは許す
            WazaKey.Left | WazaKey.Right | WazaKey.Kick,
            // キック、キックとパンチは同時に押すな、いつまでも下を押すな、右押しっぱなしは許す
            WazaKey.Down | WazaKey.Punch | WazaKey.Kick,
            // パンチ、キックとパンチは同時に押すな、いつまでも右を押すな
            WazaKey.Right | WazaKey.Punch | WazaKey.Kick,
            // 上、いつまでもキックを押すな
            WazaKey.Up | WazaKey.Kick,
        ];
        AddWaza("waza_3", waza3Command, waza3Mask);
    }

    protected override void UpdateFighter(double delta)
    {
        if (Dead)
        {
            GravityOnly(delta);
            GroundReductionAction(delta);
            _ = MoveAndSlide();
            return;
        }

        Array<int> buffer = m_Brain.GetBufferCopy();
        bool rightSide = FighterRightSide;
        bool onFloor = IsOnFloor();
        IsForwardOrBack(rightSide, out bool forward, out bool back);
        bool horizontal = forward ^ back; // 左または右の片方は横移動
        bool up = m_Brain.IsKeyPressed(WazaKey.Up, WazaKey.Up);
        bool down = m_Brain.IsKeyPressed(WazaKey.Down, WazaKey.Down);
        bool a = m_Brain.IsJustKeyPressed(WazaKey.Punch, WazaKey.Punch);
        bool b = m_Brain.IsJustKeyPressed(WazaKey.Kick, WazaKey.Kick);
        bool action = a ^ b; // パンチまたはキックの片方は攻撃
        bool punch = action & a;
        bool kick = action & b;
        bool vertical = up ^ down;
        bool jump = vertical & up;
        bool crouch = vertical & down;
        bool walk = horizontal & forward;
        bool guard = !action & horizontal & back;
        bool crouchGuard = crouch & guard;

        m_AnimationTree.Set("parameters/conditions/right_side", rightSide);
        m_AnimationTree.Set("parameters/conditions/on_floor", onFloor);
        m_AnimationTree.Set("parameters/conditions/not_on_floor", !onFloor);
        m_AnimationTree.Set("parameters/conditions/walk", walk);
        m_AnimationTree.Set("parameters/conditions/not_walk", !walk);
        m_AnimationTree.Set("parameters/conditions/guard", guard);
        m_AnimationTree.Set("parameters/conditions/not_guard", !guard);
        m_AnimationTree.Set("parameters/conditions/jump", jump);
        m_AnimationTree.Set("parameters/conditions/crouch", crouch);
        m_AnimationTree.Set("parameters/conditions/not_crouch", !crouch);
        m_AnimationTree.Set("parameters/conditions/punch", punch);
        m_AnimationTree.Set("parameters/conditions/kick", kick);
        m_AnimationTree.Set("parameters/conditions/crouch_guard", crouchGuard);

        if (State is not "waza_1" and not "waza_2" and not "waza_3" and not "damaged" and not "damaged_knock_down")
        {
            if (AnalyzeWaza("waza_3", buffer) && onFloor)
            {
                PlayRandomVoice(Waza3Voice);
                m_StateMachine.Start("waza_3");
            }
            else if (AnalyzeWaza("waza_2", buffer) && onFloor && !Waza2Enabled)
            {
                PlayRandomVoice(Waza2Voice);
                m_StateMachine.Start("waza_2");
            }
            else if (AnalyzeWaza("waza_1", buffer))
            {
                PlayRandomVoice(Waza1Voice);
                m_StateMachine.Start("waza_1");
            }
        }

        switch (State)
        {
            case "stand":

                if (jump)
                {
                    PlayActionVoice();
                    PlayRandomVoice(JumpSe);
                    JumpAction(delta);
                }
                else
                {
                    GravityOnly(delta);
                    GroundReductionAction(delta);
                }

                break;

            case "jump":

                GravityOnly(delta);
                AirWalkAction(delta, forward, back, rightSide);

                if (kick)
                {
                    PlayActionVoice();
                    PlayRandomVoice(JumpKickSe);
                }

                break;

            case "jump_kick" or "landing":

                GravityOnly(delta);
                AirWalkAction(delta, forward, back, rightSide);
                break;

            case "walk":

                GravityOnly(delta);
                GroundWalkAction(delta, forward, back, rightSide);
                break;

            case "stand_guard":

                if (jump)
                {
                    PlayActionVoice();
                    PlayRandomVoice(JumpSe);
                    JumpAction(delta);
                }
                else
                {
                    GravityOnly(delta);
                    GroundWalkAction(delta, forward, back, rightSide);
                }

                break;

            case "crouch" or "crouch_guard":

                GravityOnly(delta);
                GroundReductionAction(delta);
                CrouchAction(delta);
                break;

            default:

                GravityOnly(delta);
                ReductionAction(delta, onFloor);
                break;
        }

        _ = MoveAndSlide();
    }

    public override void Damaged(AttackArea attackArea, DamageArea damageArea, int row)
    {
        if (Dead || attackArea.Attack == 0)
        {
            return;
        }

        double life = m_LifeBar.Value;
        string state = m_StateMachine.GetCurrentNode();

        // 以下の条件を満たした場合、防御が成立する
        // 防御姿勢で
        // 背面からの攻撃ではなく
        // 防御可能な位置への攻撃である
        //     立ち防御は上段・中段を防御する
        //     しゃがみ防御は上段・下段を防御する
        // 必殺技は防御すれば防げる
        if (state is "stand_guard" && FighterRightSide != attackArea.RightSide && (row == 1 || row == 2))
        {
            AddDamageEffect(EffectGuard, damageArea);
            PlayRandomVoice(damageArea.GuardVoice);
            return;
        }
        else if (state is "crouch_guard" && FighterRightSide != attackArea.RightSide && (row == 1 || row == 3))
        {
            AddDamageEffect(EffectGuard, damageArea);
            PlayRandomVoice(damageArea.GuardVoice);
            return;
        }
        else if (state is "stand_guard" or "crouch_guard" && FighterRightSide != attackArea.RightSide)
        {
            AddDamageEffect(EffectGuard, damageArea);
            PlayRandomVoice(damageArea.GuardVoice);
            return;
        }
        else
        {
            AddDamageEffect(EffectDamage, damageArea);
            life -= attackArea.Attack;
            m_Brain.ClearBuffer();
            PlayRandomVoice(attackArea.HitVoice);
        }

        // ライフがゼロ以下になったら死亡する
        if (life <= 0)
        {
            if (state == "damaged_knock_down")
            {
                m_StateMachine.Start("dead");
            }
            else
            {
                KnockBack(attackArea.RightSide, DeadVelocity);
                m_StateMachine.Start("dead_damaged");
            }

            PlayDeadVoice();
            m_LifeBar.Value = 0;
            Dead = true;
            _ = EmitSignal(FighterRoot.SignalName.Defeated, [this]);
            // 死体には当たり判定がない
            SetCollisionLayerValue(4, false);
            SetCollisionMaskValue(4, false);
            return;
        }

        // ノックダウン中はダメージ以外何も起こらない
        if (state == "damaged_knock_down")
        {
            m_LifeBar.Value = life;
            return;
        }

        // ダメージ中にダメージを受けたらノックダウンする
        if (state == "damaged")
        {
            KnockBack(attackArea.RightSide, KnockDownVelocity);
            m_StateMachine.Travel("damaged_knock_down");
            m_LifeBar.Value = life;
            return;
        }

        // 普通にダメージを受ける
        KnockBack(attackArea.RightSide, KnockBackVelocity);
        m_StateMachine.Start("damaged");
        m_LifeBar.Value = life;
    }

    public void EffectWaza1()
    {
        if (Lib.GetPackedScene("res://mob/shot/fighter_4_shot_1.tscn") is PackedScene pack && pack.Instantiate() is ShotRoot shot)
        {
            shot.Initialize(GetNode<Node2D>("Character/AttackArea1").GlobalPosition, FighterId, FighterRightSide);
            StageRoot.AddScene(shot, "Shot");
        }
    }

    public void EffectWaza3()
    {
        if (Lib.GetPackedScene("res://mob/shot/fighter_4_shot_2.tscn") is PackedScene pack && pack.Instantiate() is Fighter4Shot2 shot)
        {
            shot.Initialize(GetNode<Node2D>("Character/AttackArea1").GlobalPosition, FighterId, FighterRightSide);
            StageRoot.AddScene(shot, "Shot");
        }
    }

    public override void ControlOuter(Array<int> command)
    {
        if (m_Brain is IOuterControledBrain brain)
        {
            brain.OuterControl(command);
        }
    }

    public override void SetStateLanding(bool force)
    {
        // CPU操作の場合、ジャンプ降下中に着陸状態へ遷移する
        if (m_Brain is IOuterControledBrain && State is "jump" && (force || 0 < Velocity.Y))
        {
            m_StateMachine.Travel("landing");
        }
    }
}
