using Godot;
using maid_by_shiraishi.data;

namespace maid_by_shiraishi.mob.fighter;

/// <summary>
/// ファイター情報
/// </summary>
public class FighterInfo : DataRoot
{
    public bool Uninhabited = false;
    public int Keyb = -1;
    public int Pad = -1;
    public int Cpu = -1;
    public int FighterNo = -1;
    public Color Color = Color.Color8(100, 100, 100);

    public FighterInfo Copy() => new()
    {
        Uninhabited = Uninhabited,
        Keyb = Keyb,
        Pad = Pad,
        Cpu = Cpu,
        Color = Color
    };
}
