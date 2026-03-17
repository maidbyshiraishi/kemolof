using Godot;
using Godot.Collections;
using kemolof.mob.fighter;
using kemolof.system;
using kemolof.system.joy_pad_controller;

namespace kemolof.screen;

/// <summary>
/// ファイター選択画面
/// </summary>
public partial class SelectFighterScreen : DialogRoot
{
    [Export]
    public string NextScreenPath { get; set; } = "res://stage/stage_{0:D4}.tscn";

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
    public int NumOfFighter = 2;

    private JoyPadController _joyPad;
    private GameDataManager _gameDataManager;
    private readonly FighterInfo[] _info = new FighterInfo[7];
    private int[] _selectedFighterNo = new int[7];

    public override void _Ready()
    {
        base._Ready();
        _joyPad = GetNode<JoyPadController>("/root/JoyPadController");
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");

        for (int i = 0; i < 7; i++)
        {
            _info[i] = _gameDataManager.GetFighterInfo(i);
            _selectedFighterNo[i] = _info[i].FighterNo;
            _info[i].FighterNo = -1;
            GetNode<Sprite2D>($"Disabled{i + 1}").Visible = _info[i].Uninhabited;
            GetNode<ColorRect>($"Select{i + 1}").Visible = !_info[i].Uninhabited;
            GetNode<ColorRect>($"Background{i + 1}").Color = _info[i].Color;
            GetNode<Label>($"Player{i + 1}").Modulate = _info[i].Color;
            GetNode<AnimatedSprite2D>($"Selected{i + 1}").Visible = false;
            GetNode<Sprite2D>($"Fighter{i + 1}").Frame = 0;
        }
    }

    public override void Active()
    {
        base.Active();
        RandomNumberGenerator rnd = new();

        for (int i = 0; i < 7; i++)
        {
            if (-1 < _info[i].Cpu)
            {
                SelectFighter(i, rnd.RandiRange(0, NumOfFighter - 1));
            }
            else
            {
                GetNode<Sprite2D>($"Fighter{i + 1}").Frame = _selectedFighterNo[i] == -1 ? 0 : _selectedFighterNo[i];
            }
        }
    }

    public override void Inactive()
    {
        base.Inactive();

        for (int i = 0; i < 7; i++)
        {
            _info[i].FighterNo = GetNode<Sprite2D>($"Fighter{i + 1}").Frame;
            _gameDataManager.SetFighterInfo(i, _info[i]);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Array<int> keyb = [];
        Array<int> pad = [];

        for (int i = 0; i < 7; i++)
        {
            if (-1 < _info[i].FighterNo)
            {
                continue;
            }

            if (!keyb.Contains(_info[i].Keyb) && -1 < _info[i].Keyb)
            {
                int index = _info[i].Keyb;
                if (UpdateSelect(
                    i,
                    Input.IsActionJustReleased($"game_keyb{index}_up"),
                    Input.IsActionJustReleased($"game_keyb{index}_down"),
                    Input.IsActionJustReleased($"game_keyb{index}_punch") || Input.IsActionJustReleased($"game_keyb{index}_kick")
                ))
                {
                    keyb.Add(_info[i].Keyb);
                }
            }
            else if (!pad.Contains(_info[i].Pad) && -1 < _info[i].Pad)
            {
                int index = _info[i].Pad;
                if (UpdateSelect(
                    i,
                    _joyPad.IsActionJustReleased(index, "joy_pad_up"),
                    _joyPad.IsActionJustReleased(index, "joy_pad_down"),
                    _joyPad.IsActionJustReleased(index, "joy_pad_punch") || _joyPad.IsActionJustReleased(index, "joy_pad_kick")
                ))
                {
                    pad.Add(_info[i].Pad);
                }
            }
        }

        if (IsSelectedAll())
        {
            int stageNo = _gameDataManager.GetStageNo();
            GetNode<DialogLayer>("/root/DialogLayer").OpenScreen(string.Format(NextScreenPath, stageNo + 1), Fadeout, Fadein);
        }
    }

    private bool IsSelectedAll()
    {
        for (int i = 0; i < 7; i++)
        {
            if (_info[i].Uninhabited)
            {
                continue;
            }

            if (_info[i].FighterNo < 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool UpdateSelect(int playerIndex, bool up, bool down, bool select)
    {
        int frame = GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame;
        bool ret = false;

        if (up)
        {
            if (0 < frame)
            {
                GetNode<SePlayer>("/root/SePlayer").Play("menu_move", true);
                GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame = frame - 1;
            }
        }
        else if (down)
        {
            if (frame < NumOfFighter - 1)
            {
                GetNode<SePlayer>("/root/SePlayer").Play("menu_move", true);
                GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame = frame + 1;
            }
        }
        else if (select)
        {
            SelectFighter(playerIndex, frame);
            ret = true;
        }

        GetNode<AnimatedSprite2D>($"Up{playerIndex + 1}").Visible = !ret && GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame != 0;
        GetNode<AnimatedSprite2D>($"Down{playerIndex + 1}").Visible = !ret && GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame != NumOfFighter - 1;
        return ret;
    }

    private void SelectFighter(int playerIndex, int fighterNo)
    {
        _info[playerIndex].FighterNo = fighterNo;
        GetNode<Sprite2D>($"Fighter{playerIndex + 1}").Frame = fighterNo;
        GetNode<SePlayer>("/root/SePlayer").Play("menu_select", true);
        GetNode<AnimatedSprite2D>($"Selected{playerIndex + 1}").Visible = true;
        GetNode<AnimatedSprite2D>($"Up{playerIndex + 1}").Visible = false;
        GetNode<AnimatedSprite2D>($"Down{playerIndex + 1}").Visible = false;
    }

    public void Up1()
    {
        _ = UpdateSelect(0, true, false, false);
    }

    public void Up2()
    {
        _ = UpdateSelect(1, true, false, false);
    }

    public void Up3()
    {
        _ = UpdateSelect(2, true, false, false);
    }

    public void Up4()
    {
        _ = UpdateSelect(3, true, false, false);
    }

    public void Up5()
    {
        _ = UpdateSelect(4, true, false, false);
    }

    public void Up6()
    {
        _ = UpdateSelect(5, true, false, false);
    }

    public void Up7()
    {
        _ = UpdateSelect(6, true, false, false);
    }

    public void Down1()
    {
        _ = UpdateSelect(0, false, true, false);
    }

    public void Down2()
    {
        _ = UpdateSelect(1, false, true, false);
    }

    public void Down3()
    {
        _ = UpdateSelect(2, false, true, false);
    }

    public void Down4()
    {
        _ = UpdateSelect(3, false, true, false);
    }

    public void Down5()
    {
        _ = UpdateSelect(4, false, true, false);
    }

    public void Down6()
    {
        _ = UpdateSelect(5, false, true, false);
    }

    public void Down7()
    {
        _ = UpdateSelect(6, false, true, false);
    }

    public void Select1()
    {
        _ = UpdateSelect(0, false, false, true);
    }

    public void Select2()
    {
        _ = UpdateSelect(1, false, false, true);
    }

    public void Select3()
    {
        _ = UpdateSelect(2, false, false, true);
    }

    public void Select4()
    {
        _ = UpdateSelect(3, false, false, true);
    }

    public void Select5()
    {
        _ = UpdateSelect(4, false, false, true);
    }

    public void Select6()
    {
        _ = UpdateSelect(5, false, false, true);
    }

    public void Select7()
    {
        _ = UpdateSelect(6, false, false, true);
    }
}
