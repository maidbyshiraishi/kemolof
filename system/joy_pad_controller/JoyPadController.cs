using Godot;
using Godot.Collections;

namespace kemolof.system.joy_pad_controller;

/// <summary>
/// 複数のゲームパッドのボタン状態を取り扱う
/// </summary>
public partial class JoyPadController : Node
{
    [Signal]
    public delegate void JoyConnectionChangedEventHandler(int device, bool connected);

    [Export]
    public string ActionNamePrefix { get; set; } = "joy_pad_";

    private readonly Mutex _mutex = new();

    private enum JustMode
    {
        Pressed,
        Released
    }

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
        _ = Input.Singleton.Connect(Input.SignalName.JoyConnectionChanged, new(this, MethodName.OnJoyConnectionChanged));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _ = CallDeferred(MethodName.UpdateInputEvent, []);
    }

    public void OnJoyConnectionChanged(int device, bool connected)
    {
        ScanDevice();
        _ = EmitSignal(SignalName.JoyConnectionChanged, [device, connected]);
    }

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

    public void ScanDevice()
    {
        _mutex.Lock();
        Array<int> devices = Input.GetConnectedJoypads();
        _deviceId = devices;
        _mutex.Unlock();
    }

    public int DeviceCount()
    {
        return _deviceId.Count;
    }

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

    private bool IsJustActionCommon(int deviceIndex, string actionName, JustMode justMode)
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
            if (inputEvent is InputEventJoypadButton button)
            {
                string id = $"{button.ButtonIndex}{device}";
                bool lastVal = _lastButton.ContainsKey(id) && _lastButton[id];
                bool nowVal = Input.IsJoyButtonPressed(device, button.ButtonIndex);

                if (justMode is JustMode.Pressed && !lastVal && nowVal)
                {
                    return true;
                }
                else if (justMode is JustMode.Released && lastVal && !nowVal)
                {
                    return true;
                }
            }

            if (inputEvent is InputEventJoypadMotion motion)
            {
                string id = $"{motion.Axis}{device}";
                float joyAxis = Input.GetJoyAxis(device, motion.Axis);

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

        return false;
    }

    public bool IsActionJustPressed(int deviceIndex, string actionName)
    {
        return IsJustActionCommon(deviceIndex, actionName, JustMode.Pressed);
    }

    public bool IsActionJustReleased(int deviceIndex, string actionName)
    {
        return IsJustActionCommon(deviceIndex, actionName, JustMode.Released);
    }
}
