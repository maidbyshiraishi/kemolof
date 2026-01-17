using Godot;

namespace kemolof.mob.shot;

/// <summary>
/// Shotインタフェース
/// </summary>
public interface IShot
{
    #region IShotインタフェース
    /// <summary>
    /// 初期化する
    /// </summary>
    /// <param name="globalPosition">生成座標</param>
    /// <param name="fighterId">ファイターID</param>
    /// <param name="rightSide">右側か</param>
    void Initialize(Vector2 globalPosition, int fighterId, bool rightSide);
    #endregion
}
