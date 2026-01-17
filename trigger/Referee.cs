using Godot;
using Godot.Collections;
using kemolof.command;
using kemolof.command.dialog;
using kemolof.mob.fighter;

namespace kemolof.trigger;

/// <summary>
/// キーの開放でコマンドを実行するトリガー
/// </summary>
public partial class Referee : Node
{
    private Array<FighterRoot> _all = [];
    private Array<FighterRoot> _loser = [];
    private bool _finished = false;
    private bool _started = false;

    public override void _Ready()
    {
        // Godotエディタからシグナルを接続すると
        // リリースビルドのエクスポート時、接続が失われることがある。
        _ = GetNodeOrNull<Timer>("Timer")?.Connect(Timer.SignalName.Timeout, new(this, MethodName.JudgeGame));
    }

    public void StartJudge(bool started)
    {
        _started = started;
    }

    public void EntryLoser(FighterRoot fighter)
    {
        _loser.Add(fighter);
    }

    public void EntryFighter(FighterRoot fighter)
    {
        _all.Add(fighter);
    }

    public void JudgeGame()
    {
        if (_finished || !_started)
        {
            return;
        }

        if (_all.Count == _loser.Count)
        {
            CommandRoot.ExecChildren(GetNodeOrNull("Draw"), this, true);
            _finished = true;
            return;
        }

        if (_all.Count - 1 == _loser.Count)
        {
            foreach (FighterRoot test in _all)
            {
                if (!_loser.Contains(test))
                {
                    OpenDialogCommand command = GetNode<OpenDialogCommand>("Winner/OpenDialogCommand");
                    command.Argument[0] = $"No.{test.FighterId + 1} {test.FighterName}";
                    command.Argument[1] = test.FighterColor;
                    CommandRoot.ExecChildren(GetNodeOrNull("Winner"), this, true);
                    _finished = true;
                    return;
                }
            }
        }
    }
}
