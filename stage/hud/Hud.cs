using Godot;

namespace kemolof.stage.hud;

/// <summary>
/// ステータス表示
/// </summary>
public partial class Hud : CanvasLayer, IGameNode
{
    [ExportGroup("Start Text")]

    [Export]
    public string Text1 { get; set; }

    [Export]
    public string Text2 { get; set; }

    [Export]
    public string Text3 { get; set; }

    public override void _Ready()
    {
        AddToGroup(IGameNode.GameNodeGroup);
    }

    public void ShowMessage()
    {
        GetNode<Label>("Start/Label_1").Text = Text1;
        GetNode<Label>("Start/Label_2").Text = Text2;
        GetNode<Label>("Start/Label_3").Text = Text3;
        GetNode<Control>("Start").Show();
    }

    public void HideMessage()
    {
        GetNode<Control>("Start").Hide();
    }

    public void SetMessage(string text1, string text2, string text3)
    {
        Text1 = string.IsNullOrWhiteSpace(text1) ? "" : text1;
        Text2 = string.IsNullOrWhiteSpace(text2) ? "" : text2;
        Text3 = string.IsNullOrWhiteSpace(text3) ? "" : text3;
    }
}
