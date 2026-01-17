using Godot;

namespace kemolof.decoration;

/// <summary>
/// 飾りエフェクト
/// </summary>
public partial class Decoration : Node2D
{
    public override void _Ready()
    {
        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<Timer>("Timer")?.Connect(Timer.SignalName.Timeout, new(this, Node.MethodName.QueueFree));
    }
}
