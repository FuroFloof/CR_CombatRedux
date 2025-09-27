using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum cr_player_hardware_brand
{
    Index,
    OculusQuest,
    Pico,
    Unknown
}

public struct cr_player_xr_controller_info
{
    public InputDevice Device;
    public XRNode Node;            // LeftHand, RightHand, or Unknown
    public cr_player_hardware_brand Brand;
    public string Name;
    public string Manufacturer;

    public override string ToString()
    {
        return $"[{Node}] {Brand} | name=\"{Name}\" manufacturer=\"{Manufacturer}\" characteristics={Device.characteristics}";
    }
}


public class cr_player_hardware : cr_MonoBehavior
{
    public static cr_player_hardware Instance { get; private set; }

    public cr_player_xr_controller_info? LeftController  => _controllers.TryGetValue(XRNode.LeftHand, out var cL)  ? cL  : (cr_player_xr_controller_info?)null;
    public cr_player_xr_controller_info? RightController => _controllers.TryGetValue(XRNode.RightHand, out var cR) ? cR : (cr_player_xr_controller_info?)null;

    // Fired whenever the set or identity of controllers changes
    public event Action<IReadOnlyDictionary<XRNode, cr_player_xr_controller_info>> OnControllersChanged;

    readonly Dictionary<XRNode, cr_player_xr_controller_info> _controllers = new Dictionary<XRNode, cr_player_xr_controller_info>();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;

        RefreshAll(); // pick up devices present at startup
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }

    // Manually force a rescan
    [ContextMenu("Refresh XR Controllers")]
    public void RefreshAll()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        _controllers.Clear();

        foreach (var dev in devices)
        {
            if (!IsHandheldController(dev)) continue;
            TryTrackDevice(dev);
        }

        NotifyChangedIfAny();
    }

    void OnDeviceConnected(InputDevice dev)
    {
        if (!IsHandheldController(dev)) return;
        if (TryTrackDevice(dev))
            NotifyChangedIfAny();
    }

    void OnDeviceDisconnected(InputDevice dev)
    {
        // Remove from either hand if it was tracked
        bool changed = false;
        foreach (var node in new[] { XRNode.LeftHand, XRNode.RightHand })
        {
            if (_controllers.TryGetValue(node, out var info) && info.Device == dev)
            {
                _controllers.Remove(node);
                changed = true;
            }
        }
        if (changed) NotifyChangedIfAny();
    }

    bool TryTrackDevice(InputDevice dev)
    {
        var node = GuessNode(dev);
        if (node == XRNode.LeftHand || node == XRNode.RightHand)
        {
            var info = BuildInfo(dev, node);
            _controllers[node] = info;
            Debug.Log($"[XR] Controller detected: {info}");
            return true;
        }
        return false;
    }

    void NotifyChangedIfAny()
    {
        OnControllersChanged?.Invoke(_controllers);
    }

    static bool IsHandheldController(InputDevice dev)
    {
        var ch = dev.characteristics;
        bool isController = (ch & InputDeviceCharacteristics.Controller) != 0;
        bool isHand       = (ch & (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Right)) != 0;
        bool isHeld       = (ch & InputDeviceCharacteristics.HeldInHand) != 0;
        return isController && (isHand || isHeld);
    }

    static XRNode GuessNode(InputDevice dev)
    {
        var ch = dev.characteristics;
        if ((ch & InputDeviceCharacteristics.Left)  != 0) return XRNode.LeftHand;
        if ((ch & InputDeviceCharacteristics.Right) != 0) return XRNode.RightHand;
        return XRNode.HardwareTracker; // Unknown hand
    }

    static cr_player_xr_controller_info BuildInfo(InputDevice dev, XRNode node)
    {
        dev.TryGetFeatureValue(new InputFeatureUsage<bool>("deviceName"), out var _); // not all runtimes populate this
        
        string name = SafeLower(dev.name);
        string mfg  = SafeLower(dev.manufacturer);

        var brand = ClassifyBrand(name, mfg);

        return new cr_player_xr_controller_info
        {
            Device = dev,
            Node = node,
            Brand = brand,
            Name = dev.name ?? "",
            Manufacturer = dev.manufacturer ?? ""
        };
    }

    static cr_player_hardware_brand ClassifyBrand(string nameLower, string mfgLower)
    {
        // Valve Index
        if (nameLower.Contains("index") || nameLower.Contains("knuckles") || mfgLower.Contains("valve"))
            return cr_player_hardware_brand.Index;

        // Oculus Quest (Meta Touch)
        if (nameLower.Contains("oculus") || nameLower.Contains("touch") || nameLower.Contains("quest") ||
            mfgLower.Contains("oculus") || mfgLower.Contains("meta"))
            return cr_player_hardware_brand.OculusQuest;

        // Pico (Neo, Pico 4, Pico 3, etc.)
        if (nameLower.Contains("pico") || nameLower.Contains("neo") || mfgLower.Contains("pico"))
            return cr_player_hardware_brand.Pico;

        return cr_player_hardware_brand.Unknown;
    }

    static string SafeLower(string s) => string.IsNullOrEmpty(s) ? "" : s.ToLowerInvariant();

    // Handy inspector buttons to print a summary
    [ContextMenu("Print Summary")]
    public void PrintSummary()
    {
        Debug.Log("=== XR Controller Summary ===");
        if (LeftController.HasValue)  Debug.Log("Left  : " + LeftController.Value);
        else                          Debug.Log("Left  : none");
        if (RightController.HasValue) Debug.Log("Right : " + RightController.Value);
        else                          Debug.Log("Right : none");
    }

}
