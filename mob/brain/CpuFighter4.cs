using kemolof.mob.fighter;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.brain;

/// <summary>
/// ファイター4CPU操作
/// </summary>
public partial class CpuFighter4 : CpuFighterRoot
{
    private SightArea _waza1Area;
    private SightArea _waza2Area;
    private SightArea _waza3Area;
    private Fighter4 _fighter4;

    public override void _Ready()
    {
        base._Ready();
        _waza1Area = GetNode<SightArea>("Waza1Area");
        _waza2Area = GetNode<SightArea>("Waza2Area");
        _waza3Area = GetNode<SightArea>("Waza3Area");

        if (GetParent() is Fighter4 fighter4)
        {
            _fighter4 = fighter4;
        }
    }

    public override void _Process(double delta)
    {
        string fighterState = Fighter.State;

        // 開始時ポーズの場合は何もしない
        if (fighterState is "mie")
        {
            return;
        }

        // アスレチックステージ用処理
        if (fighterState is "landing")
        {
            // _groundRayが反応している場合、
            if (m_GroundRay.IsColliding())
            {
                // 速度X < 0の場合、右へ
                if (Fighter.Velocity.X < 0)
                {
                    InsertBuffer(WazaKey.Right);
                    return;
                }
                // 0 < 速度Xの場合、左へ
                else if (0 < Fighter.Velocity.X)
                {
                    InsertBuffer(WazaKey.Left);
                    return;
                }

                // 速度X == 0の場合、何もしない
                return;
            }

            // _groundRayが反応していない場合、後退する
            //   戻っても陸地がない場合、仕方がないが穴に落ちる
            //   移動方向にキャラがいる場合は反転
            int direction = Fighter.FighterRightSide
                ? m_JumpRay.IsColliding() ? WazaKey.Right : WazaKey.Left
                : m_JumpRay.IsColliding() ? WazaKey.Left : WazaKey.Right;
            InsertBuffer(direction);
            return;
        }

        // Areaの向きを変える
        Scale = Fighter.FighterRightSide ? new(-1f, 1f) : new(1f, 1f);

        if (Fighter.FighterRightSide != Fighter.RightSide)
        {
            InsertBuffer(WazaKey.Free);
            return;
        }

        if (_fighter4 is not null && !_fighter4.Waza2Enabled && m_Random.Randf() <= Waza2Probability && _waza2Area.HasFighter(FighterId) is not null)
        {
            m_NextKey.Clear();
            InsertWaza2();
            return;
        }

        bool onFloor = Fighter.IsOnFloor();
        FighterRoot attackAreaEnemy = m_AttackArea.HasFighter(FighterId);

        // 外部操作キーを入力する
        if (m_OuterControlKey.Count != 0)
        {
            InsertBuffer(m_OuterControlKey[0]);
            m_OuterControlKey.RemoveAt(0);
            return;
        }

        // 防御
        if (m_Random.Randf() <= DefendProbability && onFloor)
        {
            // 背後から離攻撃を受けた場合、ジャンプ
            {
                if (m_BackArea.HasShot(FighterId) is not null || (m_BackArea.HasFighter(FighterId) is FighterRoot enemy && (enemy.State.StartsWith("waza") || enemy.State is "stand_punch" or "stand_kick" or "crouch_punch" or "crouch_kick")))
                {
                    m_NextKey.Clear();
                    InsertBuffer(WazaKey.Up);
                    return;
                }
            }

            // ショットが目前に迫った場合、行動をキャンセルして防御する
            if (m_AttackArea.HasShot(FighterId) is not null)
            {
                m_NextKey.Clear();
                InsertGuard();
                return;
            }

            // 格闘エリアから攻撃を受けた場合、行動をキャンセルして対処する
            if (attackAreaEnemy is not null)
            {
                if (fighterState is "walk")
                {
                    m_NextKey.Clear();
                }

                // 敵の必殺技は防御する
                if (attackAreaEnemy.State.StartsWith("waza"))
                {
                    m_NextKey.Clear();
                    InsertGuard();
                    return;
                }

                switch (attackAreaEnemy.State)
                {
                    // 敵がパンチ攻撃をした場合、防御
                    case "stand_punch":

                        m_NextKey.Clear();
                        InsertGuard();
                        return;

                    // 敵がキックをした場合
                    case "stand_kick":

                        // しゃがむ
                        m_NextKey.Clear();
                        InsertBuffer(WazaKey.Down);
                        return;

                    // 敵がしゃがみパンチをした場合、防御
                    case "crouch_punch":

                        m_NextKey.Clear();
                        InsertGuard();
                        return;

                    // 敵がしゃがみキックをした場合、しゃがみ防御
                    case "crouch_kick":

                        m_NextKey.Clear();
                        InsertCrouchGuard();
                        return;

                    // 敵がジャンプキックしている場合、防御
                    case "jump_kick":

                        m_NextKey.Clear();
                        InsertGuard();
                        return;
                }
            }
        }

        // 予約されているキーを入力する
        if (m_NextKey.Count != 0)
        {
            InsertBuffer(m_NextKey[0]);
            m_NextKey.RemoveAt(0);
            return;
        }

        // 必殺技1の射程距離
        if (m_Random.Randf() <= Waza1Probability && _waza1Area.HasFighter(FighterId) is not null)
        {
            InsertWaza1();
            return;
        }

        // 頭上と足元に敵がいる場合、移動する
        if ((m_Random.Randf() <= DefendProbability && m_OverheadArea.HasFighter(FighterId) is not null) || m_UnderfootArea.HasFighter(FighterId) is not null)
        {
            InsertBuffer(WazaKey.Up | (Fighter.RightSide ? WazaKey.Right : WazaKey.Left));
            return;
        }

        // 地上
        if (onFloor)
        {
            // 必殺技3の射程距離
            if (m_Random.Randf() <= Waza3Probability && _waza3Area.HasFighter(FighterId) is not null)
            {
                InsertWaza3();
                return;
            }

            // 格闘戦
            if (m_Random.Randf() <= AttackProbability && attackAreaEnemy is not null)
            {
                switch (attackAreaEnemy.State)
                {
                    // 敵がノックダウンした場合、しゃがみキック
                    case "damaged_knock_down":

                        InsertCrouchKick();
                        break;

                    // 敵がしゃがみ防御をした場合、パンチ
                    case "crouch_guard":

                        InsertBuffer(WazaKey.Punch);
                        return;

                    // 敵がジャンプした場合、ジャンプキック
                    case "jump":

                        InsertJumpKick();
                        return;

                    // 敵が立っている場合または歩いている場合、攻撃
                    case "stand" or "walk":
                        {
                            float f = m_Random.Randf();

                            if (f <= 0.45f)
                            {
                                InsertBuffer(WazaKey.Punch);
                                return;
                            }

                            if (f <= 0.9f)
                            {
                                InsertBuffer(WazaKey.Kick);
                                return;
                            }
                        }

                        break;
                }
            }

            // ランダム行動
            {
                float f = m_Random.Randf();

                // 歩く
                if (f <= 0.8f)
                {
                    InsertWalk();
                    return;
                }

                // ジャンプ
                if (f <= 1f)
                {
                    InsertBuffer(WazaKey.Up);
                    return;
                }
            }
        }
        // 空中
        else
        {
            // 攻撃範囲に敵がいる
            if (m_AttackArea.HasFighter(FighterId) is not null)
            {
                InsertJumpKick();
                return;
            }

            // ランダム行動
            {
                float f = m_Random.Randf();

                // 歩く
                if (f <= 0.6f)
                {
                    InsertWalk();
                    return;
                }
            }
        }

        // 結局、何もしない
        InsertBuffer(WazaKey.Free);
    }

    protected override void InsertWaza1()
    {
        InsertBuffer(WazaKey.Down);
        m_NextKey.Add(Fighter.RightSide ? WazaKey.Left : WazaKey.Right);
        m_NextKey.Add(WazaKey.Punch);
    }

    protected override void InsertWaza2()
    {
        InsertBuffer(WazaKey.Down);
        m_NextKey.Add(Fighter.RightSide ? WazaKey.Right : WazaKey.Left);
        m_NextKey.Add(WazaKey.Kick);
        m_NextKey.Add(Fighter.RightSide ? WazaKey.Left : WazaKey.Right);
        m_NextKey.Add(WazaKey.Down);
        m_NextKey.Add(WazaKey.Punch);
    }

    protected override void InsertWaza3()
    {
        InsertBuffer(WazaKey.Punch);
        m_NextKey.Add(WazaKey.Down);
        m_NextKey.Add(Fighter.RightSide ? WazaKey.Left : WazaKey.Right);
        m_NextKey.Add(WazaKey.Kick);
        m_NextKey.Add(WazaKey.Punch);
        m_NextKey.Add(WazaKey.Up);
    }
}
