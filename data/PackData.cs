using Godot;
using Godot.Collections;

namespace kemolof.data;

/// <summary>
/// ゲーム関連データのセット
/// </summary>
public class PackData : DataRoot
{
    public FlagData FlagData { get; set; } = new();
    public int StageNo { get; set; } = 0;

    public PackData()
    {
        StageNo = 0;
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
