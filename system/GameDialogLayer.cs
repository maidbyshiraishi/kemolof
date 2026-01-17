using kemolof.stage;

namespace kemolof.system;

/// <summary>
/// 画面遷移・ダイアログ制御
/// DialogLayerに対して、ゲームタイトル固有の拡張を行う。
/// プロジェクト設定＞グローバル＞自動読み込みで自動的に実行が開始される。
/// </summary>
public partial class GameDialogLayer : DialogLayer
{
    public GameStageRoot GetCurrentGameRoot()
    {
        return GetCurrentStageRoot() is GameStageRoot gameStageRoot ? gameStageRoot : null;
    }
}
