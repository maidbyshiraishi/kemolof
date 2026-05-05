using Godot;
using Godot.Collections;
using maid_by_shiraishi.command;
using maid_by_shiraishi.command.dialog;
using maid_by_shiraishi.mob.fighter;

namespace maid_by_shiraishi.trigger;

/// <summary>
/// ゲームの勝敗を監視するトリガー
/// </summary>
public partial class Referee : Node
{
    private Array<FighterRoot> _all = [];
    private Array<FighterRoot> _loser = [];
    private bool _finished = false;
    private bool _started = false;

    public override void _Ready() => GetNode<Timer>("Timer").Timeout += JudgeGame;

    public void StartJudge(bool started) => _started = started;

    public void EntryLoser(FighterRoot fighter) => _loser.Add(fighter);

    public void EntryFighter(FighterRoot fighter) => _all.Add(fighter);

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
