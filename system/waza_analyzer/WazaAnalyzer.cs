using Godot;
using System.Collections.Generic;

namespace kemolof.system.waza_analyzer;

/// <summary>
/// 格闘ゲームコマンドを検出する
/// </summary>
public partial class WazaAnalyzer : Node
{
    /// <summary>
    /// キー入力の猶予時間
    /// </summary>
    [Export]
    public int ExtensionTime { get; set; } = 10;

    /// <summary>
    /// タメキー入力の猶予時間
    /// </summary>
    [Export]
    public int TameExtensionTime { get; set; } = 40;

    /// <summary>
    /// タメの最短フレーム
    /// </summary>
    [Export]
    public int TameMinFrame { get; set; } = 20;

    /// <summary>
    /// タメの最長フレーム
    /// </summary>
    [Export]
    public int TameMaxFrame { get; set; } = 30;

    private readonly Dictionary<string, Waza> _waza = [];
    private int[] _lrConversionTable = new int[128];

    public override void _Ready()
    {
        MakeLrConversionTable();
    }

    /// <summary>
    /// 左右キーのビットを入れ替えるテーブルを作成する
    /// </summary>
    private void MakeLrConversionTable()
    {
        for (int i = 0; i < _lrConversionTable.Length; i++)
        {
            // 左右ビットを入れ替える
            _lrConversionTable[i] = SwapBits(i, 2, 3);
        }
    }

    /// <summary>
    /// 指定した2つの入れ替えたいビット1と入れ替えたいビット2のビットを入れ替えて返す。
    /// 元の値は変更されない。
    /// </summary>
    /// <param name="value">入れ替えたい値</param>
    /// <param name="index1">入れ替えたいビット1</param>
    /// <param name="index2">入れ替えたいビット2</param>
    /// <returns>入れ替え後の値</returns>
    private static int SwapBits(int value, int index1, int index2)
    {
        // 対象ビットを1にする
        int mask1 = 1 << index1;
        int mask2 = 1 << index2;
        // 対象ビットを調べる
        bool bit1 = (value & mask1) != 0;
        bool bit2 = (value & mask2) != 0;
        // 対象ビットをゼロにする
        value &= ~mask1;
        value &= ~mask2;

        // 対象ビットを入れ替えて書き戻す
        if (bit1)
        {
            value |= mask2;
        }

        if (bit2)
        {
            value |= mask1;
        }

        return value;
    }

    /// <summary>
    /// 必殺技を登録する。
    /// 優先順位の高い技から登録するように注意すること。
    /// </summary>
    /// <param name="wazaName">必殺技名</param>
    /// <param name="command">必殺技コマンド</param>
    /// <param name="mask">必殺技マスク</param>
    public void AddWaza(string wazaName, Godot.Collections.Array<int> command, Godot.Collections.Array<int> mask)
    {
        if (string.IsNullOrWhiteSpace(wazaName) || command is null || command.Count is 0 || mask is null || mask.Count is 0 || _waza.ContainsKey(wazaName))
        {
            return;
        }

        int commandLength = command.Count;

        if (commandLength != mask.Count)
        {
            GD.PrintErr($"{wazaName}のコマンドとマスクのサイズが一致しません。");
            return;
        }

        // キーバッファは新から旧の順序、必殺技のキー順は旧から新の順序で定義する。
        // そのため、必殺技のキー順は逆順にしてから解析する必要がある。
        // 実行の度に逆順にする必要はないので、あらかじめ逆順にしておく
        command.Reverse();
        mask.Reverse();

        // キー入力の対象ビットをそのまま判定すると技に登場するキーを同時に連打すると技が発動する。
        // コマンドのキーごとに同時押しを認めないためのキー入力に対してマスクデータを適用する。
        // 例えば、パンチ(0010)・キック(0001)のコマンドについてキー入力をパンチとキックの両ビット(0011)でマスクする。
        // これによってパンチ(0010)、キック(0011)、同時押し(0011)を区別できる。
        // ガチャ押しで発動してもいい場合は、パンチをパンチビットのみ、キックをキックビットのみでマスクすると同時押しでもお構いなしで検出する。

        bool[] tame = new bool[commandLength];
        int bufferSize = 0;

        for (int i = 0; i < command.Count; i++)
        {
            // キーのタメフラグをとりあえず除去する
            int tameRemoved = command[i] & WazaKey.RemoveTameFlagMask;
            // 元の値とタメフラグを除去した値が異なる場合はタメフラグがオン
            tame[i] = command[i] != tameRemoved;

            // タメキーである
            if (tame[i])
            {
                // コマンド解析時にはタメフラグを除去した値を使用する
                command[i] = tameRemoved;
                // タメキー入力の猶予時間を加算する
                bufferSize += TameExtensionTime;
            }
            // タメキーではない
            else
            {
                // キー入力の猶予時間を加算する
                bufferSize += ExtensionTime;
            }
        }

        // Godot.Collections.Arrayをフツーの配列に変換する
        int[] linearCommand = new int[commandLength];
        command.CopyTo(linearCommand, 0);
        int[] linearMask = new int[commandLength];
        mask.CopyTo(linearMask, 0);

        // 必殺技を登録する
        _waza.Add(wazaName, new(bufferSize, tame, linearCommand, linearMask));
    }

    /// <summary>
    /// 必殺技を解析する
    /// </summary>
    /// <param name="wazaName">必殺技名</param>
    /// <param name="buffer">キーバッファ</param>
    /// <param name="rightSide">右側か</param>
    /// <returns>必殺技が発動したか</returns>
    public bool AnalyzeWaza(string wazaName, Godot.Collections.Array<int> buffer, bool rightSide)
    {
        if (string.IsNullOrWhiteSpace(wazaName) || buffer is null || buffer.Count is 0 || _waza.Count == 0 || !_waza.TryGetValue(wazaName, out Waza waza))
        {
            return false;
        }

        // 技のデータを取り出す
        int[] wazaCommand = waza.Command;
        int commandLength = wazaCommand.Length;
        int[] wazaMask = waza.Mask;
        int wazaBufferSize = Mathf.Min(buffer.Count, waza.BufferSize);

        // キーバッファのサイズがコマンドの長さよりも小さい場合は解析失敗
        if (wazaBufferSize < commandLength)
        {
            return false;
        }

        // 猶予時間とコマンド解析位置を初期化する
        int extensionTime = 0;
        int commandIndex = 0;

        // キーバッファを先頭から最後まで調べる
        for (int bufferIndex = 0; bufferIndex < wazaBufferSize; bufferIndex++)
        {
            // 解析中のキーがタメキーの場合
            if (waza.Tame[commandIndex])
            {
                // タメ時間を初期化する
                int tameCount = 0;

                // キーバッファの現在位置から解析中のキーが連続して押されている時間を数える
                for (; bufferIndex < wazaBufferSize; bufferIndex++)
                {
                    // キーバッファに違うキーが出現した場合
                    if (wazaCommand[commandIndex] != ((rightSide ? _lrConversionTable[buffer[bufferIndex]] : buffer[bufferIndex]) & wazaMask[commandIndex]))
                    {
                        //キーバッファの検索は中断する
                        break;
                    }

                    tameCount++;
                }

                // タメ時間がTameMinFrameからTameMaxFrameの間だった場合
                if ((TameMinFrame < tameCount) && (tameCount < TameMaxFrame))
                {
                    // コマンドの次キーに進む
                    commandIndex++;

                    // コマンドの最後まで確認がとれた
                    if (commandIndex == commandLength)
                    {
                        // コマンドの解析は成功
                        return true;
                    }

                    // 次キーに進んだので猶予時間を初期化する
                    extensionTime = 0;
                    // 次のキーバッファの解析に移る
                    continue;
                }

                // 猶予時間を加算する
                extensionTime++;

                // 猶予時間がタメキー入力の猶予時間を超えた場合はキーバッファの解析を中止する
                if (TameExtensionTime < extensionTime)
                {
                    break;
                }

                // 次のキーバッファの解析に移る
                continue;
            }

            // タメキーではない場合
            // 反転フラグが立っている場合、ファイターの立ち位置によって左右ビットの反転が行われる
            // 左右ビットの反転を行った後なので、マスクの左右ビットは反転しなくてもよい
            // マスクで特定のキーだけに注目する
            if (wazaCommand[commandIndex] == ((rightSide ? _lrConversionTable[buffer[bufferIndex]] : buffer[bufferIndex]) & wazaMask[commandIndex]))
            {
                commandIndex++;

                if (commandIndex == commandLength)
                {
                    return true;
                }

                extensionTime = 0;
                continue;
            }

            extensionTime++;

            // 猶予時間がキー入力の猶予時間を超えた場合はキーバッファの解析を中止する
            if (ExtensionTime < extensionTime)
            {
                break;
            }
        }

        // キーバッファを全て確認し終えたか、中止された場合は解析失敗
        return false;
    }
}
