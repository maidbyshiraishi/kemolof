namespace kemolof.system.waza_analyzer;

/// <summary>
/// 必殺技
/// </summary>
/// <param name="bufferSize">バッファサイズ</param>
/// <param name="tame">タメキーフラグ</param>
/// <param name="command">技コマンド</param>
/// <param name="mask">技マスク</param>
public class Waza(int bufferSize, bool[] tame, int[] command, int[] mask)
{
    /// <summary>
    /// コマンドを構成するあるキーとその次キーまでの猶予時間の合計
    /// </summary>
    public int BufferSize = bufferSize;

    /// <summary>
    /// 必殺技コマンドのキーがタメキーかどうかのフラグ
    /// 通称、タメフラグ
    /// </summary>
    public bool[] Tame = tame;

    /// <summary>
    /// 必殺技コマンドのキー
    /// </summary>
    public int[] Command = command;

    /// <summary>
    /// 必殺技コマンドのキーマスク
    /// </summary>
    public int[] Mask = mask;
}
