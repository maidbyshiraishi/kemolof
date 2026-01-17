using Godot;
using Godot.Collections;
using kemolof.mob.fighter;
using kemolof.system.waza_analyzer;

namespace kemolof.mob.brain;

/// <summary>
/// 操作の親クラス
/// </summary>
public partial class BrainRoot : Node2D
{
    /// <summary>
    /// デバイス番号
    /// </summary>
    [Export]
    public int DeviceIndex { get; set; } = -1;

    /// <summary>
    /// キーバッファサイズ
    /// </summary>
    [Export]
    public int BufferSize { get; set; } = 90;

    /// <summary>
    /// 制御対象のファイター
    /// </summary>
    [Export]
    public FighterRoot Fighter { get; set; }

    /// <summary>
    /// キーバッファ
    /// </summary>
    protected Array<int> m_Buffer = [];

    private int _now;
    private int _last;

    public override void _Ready()
    {
        base._Ready();
        Fighter = GetParent<FighterRoot>();
    }

    /// <summary>
    /// キーバッファにキーデータを追加する。
    /// 処理は今フレームの終了後に行われ、現フレーム内でキーバッファが変更されることはない。
    /// </summary>
    /// <param name="key">キーデータ</param>
    public void InsertBuffer(int key)
    {
        _ = CallDeferred(MethodName.DeferredInsertBuffer, [key]);
    }

    /// <summary>
    /// 今フレームの終了後にキーバッファにキーデータを追加する。
    /// </summary>
    /// <param name="key">キーデータ</param>
    private void DeferredInsertBuffer(int key)
    {
        _last = _now;
        _now = key;
        m_Buffer.Insert(0, key);

        // バッファサイズが大きくなり過ぎた場合は古いデータを切り捨てる
        if (BufferSize < m_Buffer.Count)
        {
            _ = m_Buffer.Resize(BufferSize);
        }
    }

    /// <summary>
    /// バッファをクリアする
    /// 処理は今フレームの終了後に行われ、現フレーム内でキーバッファが変更されることはない。
    /// </summary>
    public void ClearBuffer()
    {
        _ = CallDeferred(MethodName.DeferredClearBuffer, []);
    }

    private void DeferredClearBuffer()
    {
        m_Buffer.Clear();
    }

    /// <summary>
    /// キーバッファのコピーを返す
    /// </summary>
    /// <returns>キーバッファのコピー</returns>
    public Array<int> GetBufferCopy()
    {
        return m_Buffer.Duplicate();
    }

    /// <summary>
    /// 指定キーが押されているか返す
    /// </summary>
    /// <param name="key">指定キー</param>
    /// <param name="keyMask">指定キーのマスク</param>
    /// <returns>指定キーが押されているか</returns>
    internal bool IsKeyPressed(int key, int keyMask)
    {
        return key == (_now & keyMask);
    }

    /// <summary>
    /// 指定キーが今フレームで押されたか返す
    /// </summary>
    /// <param name="key">指定キー</param>
    /// <param name="keyMask">指定キーのマスク</param>
    /// <returns>指定キーが今フレームで押されたか</returns>
    internal bool IsJustKeyPressed(int key, int keyMask)
    {
        int targetKey = key & keyMask;
        int nowKey = _now & keyMask;
        int lastKey = _last & keyMask;
        return targetKey != 0 && targetKey == nowKey && lastKey != targetKey;
    }

    /// <summary>
    /// 指定キーがちょうど今フレームで離されたか返す
    /// </summary>
    /// <param name="key">指定キー</param>
    /// <param name="keyMask">指定キーのマスク</param>
    /// <returns>指定キーが今フレームで離されたか</returns>
    internal bool IsJustKeyReleased(int key, int keyMask)
    {
        int targetKey = key & keyMask;
        int nowKey = _now & keyMask;
        int lastKey = _last & keyMask;
        return targetKey == 0 && targetKey == nowKey && lastKey != 0;
    }

    /// <summary>
    /// 今フレームで押されたキーを返す
    /// </summary>
    /// <returns>今フレームで押されたキー</returns>
    public int GetNowBuffer()
    {
        return 0 < m_Buffer.Count ? m_Buffer[0] : WazaKey.Free;
    }
}
