using System;

namespace maid_by_shiraishi.system;

public class IntRingBuffer
{
    private readonly int[] _buffer;
    private int _zeroIndex = 0;
    private readonly int _bufferSize;

    public IntRingBuffer(int size)
    {
        _buffer = new int[size];
        _bufferSize = size;
    }

    /// <summary>
    /// キーバッファのコピーを返す
    /// </summary>
    /// <returns>キーバッファのコピー</returns>
    public int[] GetCopy()
    {
        int[] copyBuffer = new int[_bufferSize];

        if (_zeroIndex == 0)
        {
            Array.Copy(_buffer, copyBuffer, _bufferSize);
        }
        else
        {
            int copyBase = _bufferSize - _zeroIndex;
            Array.Copy(_buffer, _zeroIndex, copyBuffer, 0, copyBase);
            Array.Copy(_buffer, 0, copyBuffer, copyBase, _zeroIndex);
        }

        return copyBuffer;
    }

    /// キーバッファにキーデータを追加する。
    /// </summary>
    /// <param name="key">キーデータ</param>
    public void Insert(int key)
    {
        _zeroIndex--;

        if (_zeroIndex < 0)
        {
            _zeroIndex = _bufferSize - 1;
        }

        _buffer[_zeroIndex] = key;
    }

    /// <summary>
    /// 今フレームで押されたキーを返す
    /// </summary>
    /// <returns>今フレームで押されたキー</returns>
    public int GetNow() => _buffer[_zeroIndex];

    /// <summary>
    /// バッファをクリアする
    /// </summary>
    public void Clear() => Array.Clear(_buffer);
}
