using Godot;
using Godot.Collections;

namespace maid_by_shiraishi.system.joy_pad_controller;

/// <summary>
/// 複数のゲームパッドのボタン状態を取り扱う
/// </summary>
public partial class JoyPadController : Node
{
    /// <summary>
    /// ゲームパッドが接続または接続解除された場合のシグナル
    /// </summary>
    /// <param name="device">デバイス番号</param>
    /// <param name="connected">接続または接続解除</param>
    [Signal]
    public delegate void JoyConnectionChangedEventHandler(int device, bool connected);

    /// <summary>
    /// ゲームパッドごとに入力を区別したいインプットマップのアクション名
    /// </summary>
    [Export]
    public string ActionNamePrefix { get; set; } = "joy_pad_";

    /// <summary>
    /// ゲームパッドの追加と削除はロックする
    /// </summary>
    private readonly Mutex _mutex = new();

    /// <summary>
    /// IsActionJustPressedかIsActionJustReleasedかどちらの動きとして処理するか
    /// </summary>
    private enum JustMode
    {
        Pressed,
        Released
    }

    /// <summary>
    /// 接続されているゲームパッドのデバイスIDリスト
    /// </summary>
    private Array<int> _deviceId = [];
    private Array<InputEvent> _entryKey = [];

    private Dictionary<string, Array<InputEvent>> _joyPadInput = [];
    private Dictionary<string, float> _joyPadDeadzone = [];

    private Dictionary<string, bool> _lastButton = [];
    private Dictionary<string, float> _lastMotion = [];

    public override void _Ready()
    {
        ScanDevice();
        ScanInputMap();
        Input.JoyConnectionChanged += OnJoyConnectionChanged;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // 呼び出しを今すぐ行わずにアイドル時間に延滞させる
        // 厳密にはフレーム間ではないそうだが、追加と削除によってエラーが発生しなければいい
        _ = CallDeferred(MethodName.UpdateInputEvent, []);
    }

    /// <summary>
    /// ゲームパッドが接続または接続解除された場合に実行される
    /// </summary>
    /// <param name="device">デバイスID</param>
    /// <param name="connected">接続または接続解除</param>
    public void OnJoyConnectionChanged(long device, bool connected)
    {
        ScanDevice();
        _ = EmitSignal(SignalName.JoyConnectionChanged, [device, connected]);
    }

    /// <summary>
    /// ゲームパッドごとにインプットマップで定義された入力の状態を確認する
    /// 状態を確認するインプットマップはScanInputMap()で対象となったアクション名のみとする。
    /// </summary>
    private void UpdateInputEvent()
    {
        foreach (int device in _deviceId)
        {
            foreach (InputEvent inputEvent in _entryKey)
            {
                if (inputEvent is InputEventJoypadButton button)
                {
                    string id = $"{button.ButtonIndex}{device}";
                    bool val = Input.IsJoyButtonPressed(device, button.ButtonIndex);

                    if (_lastButton.ContainsKey(id))
                    {
                        _lastButton[id] = val;
                    }
                    else
                    {
                        _lastButton.Add(id, val);
                    }
                }
                else if (inputEvent is InputEventJoypadMotion motion)
                {
                    string id = $"{motion.Axis}{device}";
                    float val = Input.GetJoyAxis(device, motion.Axis);

                    if (_lastMotion.ContainsKey(id))
                    {
                        _lastMotion[id] = val;
                    }
                    else
                    {
                        _lastMotion.Add(id, val);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 接続されているゲームパッドをスキャンし、デバイスIDリストを作成する。
    /// </summary>
    public void ScanDevice()
    {
        _mutex.Lock();
        Array<int> devices = Input.GetConnectedJoypads();
        _deviceId = devices;
        _mutex.Unlock();
    }

    /// <summary>
    /// 接続されているゲームパッドの台数を取得する
    /// ちゃんと同期をとった方がいいかもしれない。
    /// </summary>
    /// <returns>接続されているゲームパッドの台数</returns>
    public int DeviceCount() => _deviceId.Count;

    /// <summary>
    /// プロジェクト設定のインプットマップを読み込む
    /// ActionNamePrefixから始まるアクション名のみを対象とする。
    /// </summary>
    public void ScanInputMap()
    {
        _joyPadInput.Clear();
        _joyPadDeadzone.Clear();
        Array<StringName> actions = InputMap.GetActions();

        foreach (StringName action in actions)
        {
            string actionName = action.ToString();

            if (!actionName.StartsWith(ActionNamePrefix))
            {
                continue;
            }

            Array<InputEvent> events = InputMap.ActionGetEvents(action);

            foreach (InputEvent inputEvent in events)
            {
                if (inputEvent is not InputEventJoypadButton and not InputEventJoypadMotion)
                {
                    continue;
                }

                if (!_entryKey.Contains(inputEvent))
                {
                    _entryKey.Add(inputEvent);
                }

                if (!_joyPadInput.TryGetValue(actionName, out Array<InputEvent> value))
                {
                    value = [];
                    _joyPadInput.Add(actionName, value);
                }

                value.Add(inputEvent);

                if (!_joyPadDeadzone.ContainsKey(actionName))
                {
                    _joyPadDeadzone.Add(actionName, 0.5f);
                }

                if (inputEvent is InputEventJoypadMotion motion)
                {
                    _joyPadDeadzone[actionName] = motion.AxisValue * InputMap.ActionGetDeadzone(actionName);
                }
            }
        }
    }

    /// <summary>
    /// 指定したデバイスIDのゲームパッドに対し、Godot.InputのIsActionPressedっぽい動きをする
    /// ソレっぽい結果を返すのは確認したが、どの程度Godot.Inputと同じ動きなのかは検証していない。
    /// </summary>
    /// <param name="deviceIndex">デバイスID</param>
    /// <param name="actionName">アクション名</param>
    /// <returns>IsActionPressedっぽい結果</returns>
    public bool IsActionPressed(int deviceIndex, string actionName)
    {
        if (!_deviceId.Contains(deviceIndex))
        {
            return false;
        }

        if (!_joyPadInput.TryGetValue(actionName, out Array<InputEvent> events))
        {
            return false;
        }

        int device = _deviceId[deviceIndex];
        foreach (InputEvent inputEvent in events)
        {
            if (inputEvent is InputEventJoypadButton button && Input.IsJoyButtonPressed(device, button.ButtonIndex))
            {
                return true;
            }

            if (inputEvent is InputEventJoypadMotion motion)
            {
                float val = Input.GetJoyAxis(device, motion.Axis);

                if ((motion.AxisValue < 0 && val <= _joyPadDeadzone[actionName]) || (0 < motion.AxisValue && _joyPadDeadzone[actionName] <= val))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 指定したデバイスIDのゲームパッドに対し、Godot.InputのIsActionJustPressedとIsActionJustReleasedっぽい動きをする
    /// ソレっぽい結果を返すのは確認したが、どの程度Godot.Inputと同じ動きなのかは検証していない。
    /// </summary>
    /// <param name="deviceIndex">デバイスID</param>
    /// <param name="actionName">アクション名</param>
    /// <param name="justMode">IsActionJustPressedかIsActionJustReleasedか</param>
    /// <returns>IsActionJustPressedかIsActionJustReleasedっぽい結果</returns>
    private bool IsJustActionCommon(int deviceIndex, string actionName, JustMode justMode)
    {
        // 存在しないデバイスIDはfalse
        if (!_deviceId.Contains(deviceIndex))
        {
            return false;
        }

        // デバイスIDでゲームパッドの状態が取得できないならfalse
        if (!_joyPadInput.TryGetValue(actionName, out Array<InputEvent> events))
        {
            return false;
        }

        // デバイスIDごとに入力状態を更新する
        int device = _deviceId[deviceIndex];

        // ScanInputMapで読み込まれたインプットマップを更新する
        foreach (InputEvent inputEvent in events)
        {
            // InputEventJoypadButtonの場合
            if (inputEvent is InputEventJoypadButton button)
            {
                string id = $"{button.ButtonIndex}{device}";
                bool lastVal = _lastButton.ContainsKey(id) && _lastButton[id];
                bool nowVal = Input.IsJoyButtonPressed(device, button.ButtonIndex);

                // モードによってちょっと違う
                // IsActionJustPressedは前回falseで今回true
                if (justMode is JustMode.Pressed && !lastVal && nowVal)
                {
                    return true;
                }
                // IsActionJustReleasedは前回trueで今回false
                else if (justMode is JustMode.Released && lastVal && !nowVal)
                {
                    return true;
                }
            }

            // InputEventJoypadMotionの場合
            if (inputEvent is InputEventJoypadMotion motion)
            {
                string id = $"{motion.Axis}{device}";
                float joyAxis = Input.GetJoyAxis(device, motion.Axis);

                // InputEventJoypadMotionは-1.0から1.0の範囲
                // ゼロは動いてないからifを抜けてfalse
                // 正負によって場合分けする。もっと効率的な書き方にしたい人は書き換えよう。
                if (motion.AxisValue < 0)
                {
                    bool lastVal = (_lastMotion.TryGetValue(id, out float value) ? value : 0f) <= _joyPadDeadzone[actionName];
                    bool nowVal = joyAxis <= _joyPadDeadzone[actionName];

                    if (justMode is JustMode.Pressed && !lastVal && nowVal)
                    {
                        return true;
                    }
                    else if (justMode is JustMode.Released && lastVal && !nowVal)
                    {
                        return true;
                    }
                }
                else if (0 < motion.AxisValue)
                {
                    bool lastVal = _joyPadDeadzone[actionName] <= (_lastMotion.TryGetValue(id, out float value) ? value : 0f);
                    bool nowVal = _joyPadDeadzone[actionName] <= joyAxis;

                    if (justMode is JustMode.Pressed && !lastVal && nowVal)
                    {
                        return true;
                    }
                    else if (justMode is JustMode.Released && lastVal && !nowVal)
                    {
                        return true;
                    }
                }
            }
        }

        // 何事もなかったのでfalse
        return false;
    }

    /// <summary>
    /// 指定したデバイスIDのゲームパッドに対し、Godot.InputのIsActionJustPressedっぽい動きをする
    /// IsJustActionCommonをIsActionJustPressedモードで実行する
    /// ソレっぽい結果を返すのは確認したが、どの程度Godot.Inputと同じ動きなのかは検証していない。
    /// </summary>
    /// <param name="deviceIndex">デバイスID</param>
    /// <param name="actionName">アクション名</param>
    /// <returns>InputのIsActionJustPressedっぽい結果</returns>
    public bool IsActionJustPressed(int deviceIndex, string actionName) => IsJustActionCommon(deviceIndex, actionName, JustMode.Pressed);

    /// <summary>
    /// 指定したデバイスIDのゲームパッドに対し、Godot.IsActionJustReleased
    /// IsJustActionCommonをIsActionJustReleasedモードで実行する
    /// ソレっぽい結果を返すのは確認したが、どの程度Godot.Inputと同じ動きなのかは検証していない。
    /// </summary>
    /// <param name="deviceIndex">デバイスID</param>
    /// <param name="actionName">アクション名</param>
    /// <returns>InputのIsActionJustReleasedっぽい結果</returns>
    public bool IsActionJustReleased(int deviceIndex, string actionName) => IsJustActionCommon(deviceIndex, actionName, JustMode.Released);
}
