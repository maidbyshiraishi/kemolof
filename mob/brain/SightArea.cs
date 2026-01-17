using Godot;
using Godot.Collections;
using kemolof.mob.fighter;
using kemolof.mob.shot;

namespace kemolof.mob.brain;

/// <summary>
/// 視界
/// </summary>
public partial class SightArea : Area2D
{
    /// <summary>
    /// Area2Dと接触する
    /// </summary>
    [Export]
    public bool ConnectArea { get; set; } = true;

    /// <summary>
    /// Node2Dと接触する
    /// </summary>
    [Export]
    public bool ConnectNode { get; set; } = true;

    private readonly Array<Area2D> _area = [];
    private readonly Array<Node2D> _node = [];

    public override void _Ready()
    {
        if (ConnectArea)
        {
            _ = Connect(Area2D.SignalName.AreaEntered, new(this, MethodName.Area2DEntered));
            _ = Connect(Area2D.SignalName.AreaExited, new(this, MethodName.Area2DExited));
        }

        if (ConnectNode)
        {
            _ = Connect(Area2D.SignalName.BodyEntered, new(this, MethodName.Node2DEntered));
            _ = Connect(Area2D.SignalName.BodyExited, new(this, MethodName.Node2DExited));
        }
    }

    public ShotRoot HasShot(int fighterId)
    {
        foreach (Area2D area in _area)
        {
            if (IsInstanceValid(area) && area is ShotRoot shot && shot.FighterId != fighterId)
            {
                return shot;
            }
        }

        return null;
    }

    public FighterRoot HasFighter(int fighterId)
    {
        foreach (Node2D ndoe in _node)
        {
            if (IsInstanceValid(ndoe) && ndoe is FighterRoot fighter && fighter.FighterId != fighterId)
            {
                return fighter;
            }
        }

        return null;
    }

    public override void _Process(double delta)
    {
        if (ConnectArea)
        {
            Array<Area2D> removeArea = [];

            foreach (Area2D area in _area)
            {
                if (!IsInstanceValid(area))
                {
                    removeArea.Add(area);
                }
            }

            foreach (Area2D area in removeArea)
            {
                _ = _area.Remove(area);
            }
        }

        if (ConnectNode)
        {
            Array<Node2D> removeNode = [];

            foreach (Node2D node in _node)
            {
                if (!IsInstanceValid(node))
                {
                    removeNode.Add(node);
                }
            }

            foreach (Node2D node in removeNode)
            {
                _ = _node.Remove(node);
            }
        }
    }

    public void Area2DEntered(Area2D area)
    {
        _ = CallDeferred(MethodName.DeferredArea2DEntered, area);
    }

    public void Area2DExited(Area2D area)
    {
        _ = CallDeferred(MethodName.DeferredArea2DExited, area);
    }

    public void DeferredArea2DEntered(Area2D area)
    {
        if (!IsInstanceValid(area))
        {
            return;
        }

        if (!_area.Contains(area) && IsInstanceValid(area))
        {
            _area.Add(area);
        }
    }

    public void DeferredArea2DExited(Area2D area)
    {
        if (!IsInstanceValid(area))
        {
            return;
        }

        if (_area.Contains(area))
        {
            _ = _area.Remove(area);
        }
    }

    public void Node2DEntered(Node2D node)
    {
        _ = CallDeferred(MethodName.DeferredNode2DEntered, node);
    }

    public void Node2DExited(Node2D node)
    {
        _ = CallDeferred(MethodName.DeferredNode2DExited, node);
    }

    public void DeferredNode2DEntered(Node2D node)
    {
        if (!IsInstanceValid(node))
        {
            return;
        }

        if (!_node.Contains(node))
        {
            _node.Add(node);
        }
    }

    public void DeferredNode2DExited(Node2D node)
    {
        if (_node.Contains(node))
        {
            _ = _node.Remove(node);
        }
    }
}
