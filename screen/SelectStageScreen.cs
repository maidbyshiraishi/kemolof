using Godot;
using Godot.Collections;
using kemolof.system;

namespace kemolof.screen;

/// <summary>
/// ステージ選択画面
/// </summary>
public partial class SelectStageScreen : DialogRoot
{
    [Export]
    public string NextScreenPath { get; set; } = "res://screen/select_fighter_screen.tscn";

    /// <summary>
    /// フェードアウトエフェクト
    /// </summary>
    [Export]
    public string Fadeout { get; set; } = "fadeout_1";

    /// <summary>
    /// フェードインエフェクト
    /// </summary>
    [Export]
    public string Fadein { get; set; } = "fadein_1";

    [Export]
    public Array<string> StageList { get; set; } = [];

    [Export]
    public Array<int> StageIndex { get; set; } = [];
    public Array<int> RandomIndex { get; set; } = [];

    private AnimatedSprite2D _stageList;
    private GameDataManager _gameDataManager;
    private readonly Mutex _mutex = new();

    public override void _Ready()
    {
        base._Ready();
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        _stageList = GetNode<AnimatedSprite2D>("StageList");

        foreach (int item in StageIndex)
        {
            if (-1 < item)
            {
                RandomIndex.Add(item);
            }
        }
    }

    public override void Active()
    {
        base.Active();
        _stageList.Frame = _gameDataManager.GetStageFrame();
        UpdateFocus();
    }

    public override void Inactive()
    {
        base.Inactive();
        int stageFrame = _stageList.Frame;
        _ = StageList.Count;
        int stageNo = StageIndex[stageFrame];

        if (stageNo < 0)
        {
            stageNo = RandomIndex.PickRandom();
        }

        _gameDataManager.SetStageNo(stageNo - 1);
        _gameDataManager.SetStageFrame(stageFrame);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustReleased("ui_left"))
        {
            GetNode<SePlayer>("/root/SePlayer").Play("menu_move", true);
            BackStageIndex();
        }
        else if (Input.IsActionJustReleased("ui_right"))
        {
            GetNode<SePlayer>("/root/SePlayer").Play("menu_move", true);
            NextStageIndex();
        }
        else if (Input.IsActionJustReleased("ui_accept") || Input.IsActionJustReleased("ui_select"))
        {
            GetNode<SePlayer>("/root/SePlayer").Play("menu_select", true);
            GetNode<DialogLayer>("/root/DialogLayer").OpenScreen("res://screen/select_fighter_screen.tscn", Fadeout, Fadein);
        }
    }

    private void UpdateFocus()
    {
        int count = StageList.Count;

        if (count == 1)
        {
            GetNode<AnimatedSprite2D>("Right").Visible = false;
            GetNode<AnimatedSprite2D>("Left").Visible = false;
            GetNode<Label>("StageName").Text = StageList[0];
            return;
        }

        if (_stageList.Frame == 0)
        {
            GetNode<AnimatedSprite2D>("Right").Visible = true;
            GetNode<AnimatedSprite2D>("Left").Visible = false;
            GetNode<Label>("StageName").Text = StageList[0];
            return;
        }

        if (_stageList.Frame == count - 1)
        {
            GetNode<AnimatedSprite2D>("Right").Visible = false;
            GetNode<AnimatedSprite2D>("Left").Visible = true;
            GetNode<Label>("StageName").Text = StageList[count - 1];
            return;
        }

        GetNode<AnimatedSprite2D>("Right").Visible = true;
        GetNode<AnimatedSprite2D>("Left").Visible = true;
        GetNode<Label>("StageName").Text = StageList[_stageList.Frame];
    }

    public void NextStageIndex()
    {
        _mutex.Lock();
        int count = StageList.Count;

        if (_stageList.Frame < count)
        {
            _stageList.Frame++;
        }

        _mutex.Unlock();
        UpdateFocus();
    }

    public void BackStageIndex()
    {
        _mutex.Lock();

        if (0 < _stageList.Frame)
        {
            _stageList.Frame--;
        }

        _mutex.Unlock();
        UpdateFocus();
    }
}
