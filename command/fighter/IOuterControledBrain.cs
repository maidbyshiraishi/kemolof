using Godot.Collections;

namespace kemolof.command.fighter;

/// <summary>
/// ファイター外部制御インタフェース
/// </summary>
public interface IOuterControledBrain
{
    #region ファイター外部制御インタフェース
    /// <summary>
    /// ファイターを外部から制御する
    /// </summary>
    /// <param name="command">コマンド列</param>
    void OuterControl(Array<int> command);
    #endregion
}
