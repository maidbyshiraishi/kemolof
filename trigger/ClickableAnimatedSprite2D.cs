using Godot;

namespace kemolof.trigger;

/// <summary>
/// クリック可能なAnimatedSprite2D
/// </summary>
public partial class ClickableAnimatedSprite2D : AnimatedSprite2D
{
    [Signal]
    public delegate void PressedEventHandler();

    [Signal]
    public delegate void MouseEnteredEventHandler();

    [Signal]
    public delegate void MouseExitedEventHandler();

    private bool _last = false;

    private Rect2 GetRect()
    {
        if (SpriteFrames is null || string.IsNullOrWhiteSpace(Animation) || SpriteFrames.GetFrameTexture(Animation, Frame) is not Texture2D tex)
        {
            return new Rect2();
        }

        Vector2 size = tex.GetSize() * Scale;
        Vector2 center = Position + Offset;
        return new Rect2(center - (size / 2f), size);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            Rect2 rect = GetRect();
            bool now = rect.HasPoint(motion.Position);

            if (_last && !now)
            {
                _ = EmitSignal(SignalName.MouseExited);
            }
            else if (!_last && now)
            {
                _ = EmitSignal(SignalName.MouseEntered);
            }

            _last = now;
        }

        if (_last && @event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.IsPressed())
            {
                _ = EmitSignal(SignalName.Pressed);
            }
        }
    }
}
