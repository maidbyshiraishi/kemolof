using Godot;
using Godot.Collections;
using kemolof.mob.fighter;

namespace kemolof.data;

/// <summary>
/// ゲーム関連データのセット
/// </summary>
public class PackData : DataRoot
{
    public FighterInfo[] FighterInfo { get; set; }
    public FlagData FlagData { get; set; } = new();
    public int StageNo { get; set; } = 0;
    public int StageFrame { get; set; } = 0;

    public PackData()
    {
        StageNo = 0;
        StageFrame = 0;
        FighterInfo = new FighterInfo[7];

        for (int i = 0; i < 7; i++)
        {
            FighterInfo[i] = new FighterInfo();
        }
    }

    public void StartNewGame()
    {
        Backup();
    }

    public void Backup()
    {
    }

    public void Restore()
    {
    }

    public override Error SetConfigFile(ConfigFile file)
    {
        return FlagData.SetConfigFile(file);
    }

    public override Error GetConfigFile(ConfigFile file)
    {
        return FlagData.GetConfigFile(file);
    }

    public override Error CheckNecessaryKey(ConfigFile file)
    {
        return FlagData.CheckNecessaryKey(file);
    }

    public override void RemoveIllegalKey(ConfigFile file)
    {
        FlagData.RemoveIllegalKey(file);
    }

    public override string[] GetSectionKeys(ConfigFile file)
    {
        return FlagData.GetSectionKeys(file);
    }

    public override Array GetSectionValues(ConfigFile file)
    {
        return FlagData.GetSectionValues(file);
    }
}
