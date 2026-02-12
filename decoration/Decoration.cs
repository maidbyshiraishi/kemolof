using Godot;

namespace kemolof.decoration;

/// <summary>
/// 飾りエフェクト
/// </summary>
public partial class Decoration : Node2D
{
    public override void _Ready() => GetNodeOrNull<Timer>("Timer").Timeout += QueueFree;
}
