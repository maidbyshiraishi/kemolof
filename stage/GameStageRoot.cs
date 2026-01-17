using Godot;
using Godot.Collections;
using kemolof.mob.brain;
using kemolof.mob.fighter;
using kemolof.system;
using kemolof.trigger;

namespace kemolof.stage;

/// <summary>
/// ゲームステージの親
/// </summary>
public partial class GameStageRoot : StageRoot
{
    private readonly FighterInfo[] _info = new FighterInfo[7];
    private Referee _referee;

    public override void _Ready()
    {
        GameDataManager gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        _referee = GetNode<Referee>("Scenario/Referee");
        FighterList fighterList = GetNode<FighterList>("Fighter");
        string name = "";

        for (int i = 0; i < 7; i++)
        {
            _info[i] = gameDataManager.GetFighterInfo(i);
            BrainRoot brain = null;
            string brainPath = null;
            int deviceIndex = -1;

            if (-1 < _info[i].Keyb)
            {
                deviceIndex = _info[i].Keyb;
                name = $"キーボード{deviceIndex + 1}";
                brainPath = "res://mob/brain/human_keyb.tscn";
            }
            else if (-1 < _info[i].Pad)
            {
                deviceIndex = _info[i].Pad;
                name = $"ゲームパッド{deviceIndex + 1}";
                brainPath = "res://mob/brain/human_pad.tscn";
            }
            else if (-1 < _info[i].Cpu)
            {
                string[] power = ["弱い", "普通", "強い"];
                deviceIndex = _info[i].Cpu;
                name = $"CPU {power[deviceIndex]}";
                brainPath = $"res://mob/brain/cpu_fighter_{_info[i].FighterNo + 1}_l{deviceIndex + 1}.tscn";
            }

            if (deviceIndex < 0 || string.IsNullOrWhiteSpace(brainPath))
            {
                continue;
            }

            if (Lib.GetPackedScene(brainPath) is PackedScene packBrain && packBrain.Instantiate() is BrainRoot brainRoot)
            {
                brain = brainRoot;
                brain.DeviceIndex = deviceIndex;
            }

            if (brain is null)
            {
                continue;
            }

            if (Lib.GetPackedScene($"res://mob/fighter/fighter_{_info[i].FighterNo + 1}.tscn") is PackedScene pack && pack.Instantiate() is FighterRoot fighter)
            {
                _referee.EntryFighter(fighter);
                _ = fighter.Connect(FighterRoot.SignalName.Defeated, new Callable(_referee, Referee.MethodName.EntryLoser));
                fighter.StageRoot = this;
                fighter.Name = $"Fighter_{i + 1}";
                fighter.FighterName = $"{fighter.FighterName} ({name})";
                fighter.FighterId = i;
                fighter.FighterColor = _info[i].Color;
                Vector2 spawnPosition = GetNode<Marker2D>($"Spawn/Player_{i + 1}").GlobalPosition;
                fighter.GlobalPosition = spawnPosition;
                Vector2 respawnPosition = GetNode<Marker2D>($"Respawn/Player_{i + 1}").GlobalPosition;
                fighter.RespawnPosition = respawnPosition;
                fighter.AddChild(brain);
                fighterList.AddChild(fighter);
            }
        }

        fighterList.UpdateFighterList();
        Array<FighterRoot> fighters = fighterList.GetFighters();

        foreach (FighterRoot fighter in fighters)
        {
            fighter.SetStartDirection();
        }

        Camera camera = GetNode<Camera>("%Camera");
        camera.Enabled = true;
        _referee.StartJudge(true);
    }

    public override void InitializeNode()
    {
        GetNode<GameDataManager>("/root/GameDataManager").Restore();
        base.InitializeNode();
    }

    public void AddScene(Node node, string parentNodeName)
    {
        if (GetNodeOrNull(parentNodeName) is Node parentNode)
        {
            AddSceneToNode(node, parentNode);
        }
    }

    public void AddSceneToNode(Node node, Node parentNode)
    {
        _ = CallDeferred(MethodName.DeferredAddSceneToNode, [node, parentNode]);
    }

    private static void DeferredAddSceneToNode(Node node, Node parentNode)
    {
        parentNode.AddChild(node);
        InitializeNodeAll(node);
    }

    private static void InitializeNodeAll(Node root)
    {
        if (root is null)
        {
            return;
        }

        if (root is IGameNode inode)
        {
            inode.InitializeNode();
        }

        foreach (Node n in root.GetChildren())
        {
            InitializeNodeAll(n);
        }
    }

    public void ReparentNode(Node2D node, string nodeName)
    {
        _ = node?.CallDeferred(Node.MethodName.Reparent, [GetNode(nodeName)]);
    }

    public void DamageAllFighters(int damage, Array<string> hitVoice)
    {
        FighterList fighterList = GetNode<FighterList>("Fighter");
        fighterList.DamageAllFighters(damage, hitVoice);
    }
}
