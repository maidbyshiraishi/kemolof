using Godot;
using kemolof.data;

namespace kemolof.mob.fighter;

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

    public FighterInfo Copy()
    {
        return new()
        {
            Uninhabited = Uninhabited,
            Keyb = Keyb,
            Pad = Pad,
            Cpu = Cpu,
            Color = Color
        };
    }
}
