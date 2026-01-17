namespace kemolof.system.waza_analyzer;

public partial class WazaKey
{
    // 0b_0000_0000_0-タメ-キック-パンチ_右-左-下-上

    /// <summary>
    /// キーを押していない
    /// </summary>
    public static readonly int Free = 0b_0000_0000_0000_0000;

    /// <summary>
    /// 上キー
    /// </summary>
    public static readonly int Up = 0b_0000_0000_0000_0001;

    /// <summary>
    /// 下キー
    /// </summary>
    public static readonly int Down = 0b_0000_0000_0000_0010;

    /// <summary>
    /// 左キー
    /// </summary>
    public static readonly int Left = 0b_0000_0000_0000_0100;

    /// <summary>
    /// 右キー
    /// </summary>
    public static readonly int Right = 0b_0000_0000_0000_1000;

    /// <summary>
    /// パンチキー
    /// </summary>
    public static readonly int Punch = 0b_0000_0000_0001_0000;

    /// <summary>
    /// キックキー
    /// </summary>
    public static readonly int Kick = 0b_0000_0000_0010_0000;

    /// <summary>
    /// タメキーフラグ
    /// </summary>
    public static readonly int Tame = 0b_0000_0000_0100_0000;

    /// <summary>
    /// タメキーフラグを消すマスク
    /// </summary>
    public static readonly int RemoveTameFlagMask = 0b_0000_0000_0011_1111;
}
