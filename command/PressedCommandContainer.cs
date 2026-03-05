using Godot;
using static Godot.Control;

namespace kemolof.command;

/// <summary>
/// 押下コマンドコンテナ
/// </summary>
public partial class PressedCommandContainer : CommandContainer
{
    private Control _control;
    private CanvasItem _canvasItem;

    public override void _Ready()
    {
        base._Ready();
        Node node = GetParent();

        if (node is Control control && control is BaseButton baseButton)
        {
            _control = control;
            baseButton.Pressed += Pressed;
            return;
        }

        if (node is CanvasItem canvasItem && node.HasSignal("Pressed"))
        {
            _canvasItem = canvasItem;
            _ = _canvasItem.Connect("Pressed", new(this, MethodName.Pressed));
            return;
        }
    }

    public virtual void Pressed()
    {
        if (_control is not null && _control.FocusMode != FocusModeEnum.None)
        {
            ExecAllCommand(this, _control, true);
            return;
        }

        if (_canvasItem is not null && _canvasItem.IsVisibleInTree())
        {
            ExecAllCommand(this, _canvasItem, true);
            return;
        }
    }
}
