using Godot;
using Godot.Collections;
using kemolof.mob;
using kemolof.mob.fighter;

namespace kemolof.stage;

public partial class FighterList : Node2D
{
    private Array<FighterRoot> _fighter = [];

    public Array<FighterRoot> GetAvailableFighters()
    {
        return _fighter;
    }

    public override void _Process(double delta)
    {
        _ = CallDeferred(MethodName.UpdateFighterList, []);
    }

    public void UpdateFighterList()
    {
        Array<Node> children = GetChildren();
        _fighter.Clear();

        foreach (Node node in children)
        {
            if (node is FighterRoot fighter && !fighter.Dead)
            {
                _fighter.Add(fighter);
            }
        }
    }

    public Array<FighterRoot> GetFighters()
    {
        return _fighter;
    }

    public void DamageAllFighters(int damage, Array<string> hitVoice)
    {
        AttackArea attackArea = new()
        {
            FighterId = int.MaxValue,
            Attack = damage,
            HitVoice = hitVoice
        };

        foreach (FighterRoot fighter in _fighter)
        {
            fighter.Damaged(attackArea, fighter.GetNode<DamageArea>("Character/DamageArea1"), 0);
        }
    }

    public FighterRoot GetNeighborFighter(FighterRoot fighter)
    {
        float distance = float.MaxValue;
        FighterRoot ret = null;

        foreach (FighterRoot target in _fighter)
        {
            if (target == fighter || fighter.Dead)
            {
                continue;
            }

            float distanceTo = Mathf.Abs(target.GlobalPosition.X - fighter.GlobalPosition.X);

            if (distanceTo < distance)
            {
                distance = distanceTo;
                ret = target;
            }
        }

        return ret;
    }
}
