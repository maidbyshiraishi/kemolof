using Godot;
using Godot.Collections;

namespace kemolof.mob;

/// <summary>
/// ダメージ判定
/// </summary>
public partial class DamageArea : Area2D
{
    [Signal]
    public delegate void DamagedEventHandler(AttackArea attackArea, DamageArea damageArea, int row);

    [Export]
    public int FighterId { get; set; }

    [Export]
    public int Row { get; set; } = 0;

    [Export]
    public Array<string> GuardVoice { get; set; } = [];

    private CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        _ = Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.Hit));
    }

    public void Hit(Area2D area)
    {
        if (area is not AttackArea attackArea || attackArea.FighterId == FighterId)
        {
            return;
        }

        _ = EmitSignal(SignalName.Damaged, [attackArea, this, Row]);
    }
}
