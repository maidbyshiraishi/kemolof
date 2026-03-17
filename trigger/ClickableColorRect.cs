using Godot;

namespace kemolof.trigger;

/// <summary>
/// クリック可能なColorRect
/// </summary>
public partial class ClickableColorRect : ColorRect
{
    [Signal]
    public delegate void PressedEventHandler();

    [Signal]
    public delegate void MouseEnteredEventHandler();

    [Signal]
    public delegate void MouseExitedEventHandler();

    private bool _last = false;

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

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
