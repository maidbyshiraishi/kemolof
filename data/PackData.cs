using Godot;
using Godot.Collections;

namespace maid_by_shiraishi.data;

/// <summary>
/// ゲーム関連データのセット
/// </summary>
public class PackData : DataRoot
{
    public FlagData FlagData { get; set; } = new();

    public int StageNo { get; set; } = 0;

    public PackData() => StageNo = 0;

    public void StartNewGame() => Backup();

    public void Backup()
    {
    }

    public void Restore()
    {
    }

    public override Error SetConfigFile(ConfigFile file) => FlagData.SetConfigFile(file);

    public override Error GetConfigFile(ConfigFile file) => FlagData.GetConfigFile(file);

    public override Error CheckNecessaryKey(ConfigFile file) => FlagData.CheckNecessaryKey(file);

    public override void RemoveIllegalKey(ConfigFile file) => FlagData.RemoveIllegalKey(file);

    public override string[] GetSectionKeys(ConfigFile file) => FlagData.GetSectionKeys(file);

    public override Array GetSectionValues(ConfigFile file) => FlagData.GetSectionValues(file);
}
