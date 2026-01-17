using Godot;
using kemolof.system;

namespace kemolof.decoration;

/// <summary>
/// フローティングメッセージ
/// </summary>
public partial class FloatingMessage : Node2D
{
    [Export]
    public string Text { get; set; } = "text";

    [Export]
    public Color Color { get; set; } = Colors.White;

    [Export]
    public string SeName { get; set; }

    public override void _Ready()
    {
        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.AnimationFinished));

        GetNode<Label>("Label").Text = Text;
        GetNode<Label>("Label").SelfModulate = Color;
        GetNode<SePlayer>("/root/SePlayer").Play(SeName);
        GetNode<AnimationPlayer>("AnimationPlayer").Play("floating_message");
    }

    public void AnimationFinished(StringName animName)
    {
        QueueFree();
    }
}
