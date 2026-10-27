using Godot;

namespace maid_by_shiraishi.trigger;

/// <summary>
/// クリック可能なColorRect
/// </summary>
public partial class ClickableColorRect : ColorRect
{
    [Signal]
    public delegate void PressedEventHandler();

    private bool _last = false;

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        Rect2 rect = GetRect();

        if (@event is InputEventMouseMotion motion)
        {
            bool now = rect.HasPoint(motion.Position);

            if (_last && !now)
            {
                _ = EmitSignal(Control.SignalName.MouseExited);
            }
            else if (!_last && now)
            {
                _ = EmitSignal(Control.SignalName.MouseEntered);
            }

            _last = now;
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            bool now = rect.HasPoint(mouseButton.Position);

            if (now && mouseButton.IsPressed())
            {
                _ = EmitSignal(SignalName.Pressed);
            }
        }
    }
}
