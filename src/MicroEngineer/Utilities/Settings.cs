using ReduxLib.Configuration;
using UnityEngine;

namespace MicroEngineer.Utilities;

public static class Settings
{
    public static MicroEngineerPlugin Plugin => MicroEngineerPlugin.Instance;
    
    public static ConfigValue<bool> EnableKeybinding;
    public static ConfigValue<KeyCode> Keybind1;
    public static ConfigValue<KeyCode> Keybind2;
    public static ConfigValue<int> MainUpdateLoopUpdateFrequency;
    public static ConfigValue<int> StageInfoUpdateFrequency;

    public static void Initialize()
    {
        EnableKeybinding = new(Plugin.SWConfiguration.Bind(
            "Keybinding",
            "Enable keybinding",
            true,
            "Enables or disables keyboard shortcuts to show or hide windows in Flight and OAB scenes."
        ));
            
        Keybind1 = new(Plugin.SWConfiguration.Bind(
            "Keybinding",
            "Keycode 1",
            KeyCode.LeftControl,
            "First keycode."
        ));
            
        Keybind2 = new(Plugin.SWConfiguration.Bind(
            "Keybinding",
            "Keycode 2",
            KeyCode.E,
            "Second keycode."));
            
        MainUpdateLoopUpdateFrequency = new(Plugin.SWConfiguration.Bind(
            "Update Frequency (ms)",
            "Main Update",
            100,
            "Time in milliseconds between every entry refresh.\n\nIncrease the value for better performance at the cost of longer time between updates.",
            new RangeConstraint<int>(0,1000)
        ));
            
        StageInfoUpdateFrequency = new(Plugin.SWConfiguration.Bind(
            "Update Frequency (ms)",
            "Stage Info",
            500,
            "Time in milliseconds between every Stage Info refresh.\n\nIncrease the value for better performance at the cost of longer time between updates.",
            new RangeConstraint<int>(0,1000)
        ));
    }
}