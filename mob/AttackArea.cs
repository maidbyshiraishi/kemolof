using Godot;
using Godot.Collections;

namespace kemolof.mob;

/// <summary>
/// 攻撃判定
/// </summary>
public partial class AttackArea : Area2D
{
    [Export]
    public int FighterId { get; set; }

    [Export]
    public int Attack { get; set; } = 1;

    [Export]
    public bool RightSide { get; set; } = false;

    [Export]
    public Array<string> HitVoice { get; set; } = [];
}
