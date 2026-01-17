using Godot;

namespace kemolof.mob.shot;

/// <summary>
/// スク水必殺技2
/// </summary>
public partial class Fighter3Shot2 : ShotRoot
{
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        base._Ready();

        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<Timer>("Timer")?.Connect(Timer.SignalName.Timeout, new(this, MethodName.WakeUpShot));
    }

    public void WakeUpShot()
    {
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Show();
        GetNode<CollisionShape2D>("AttackArea1/AttackCollision").Disabled = false;
        GetNode<CollisionShape2D>("EraseCollision/CollisionShape2D").Disabled = false;
        SetPhysicsProcess(true);
    }
}
