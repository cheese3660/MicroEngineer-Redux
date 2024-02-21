using BepInEx.Configuration;
using UnityEngine;

namespace MicroEngineer.Utilities;

public static class Settings
{
    public static MicroEngineerPlugin Plugin => MicroEngineerPlugin.Instance;
    
    public static ConfigEntry<bool> EnableKeybinding;
    public static ConfigEntry<KeyCode> Keybind1;
    public static ConfigEntry<KeyCode> Keybind2;
    public static ConfigEntry<int> MainUpdateLoopUpdateFrequency;
    public static ConfigEntry<int> StageInfoUpdateFrequency;

    public static void Initialize()
    {
        EnableKeybinding = Plugin.Config.Bind(
            "Keybinding",
            "Enable keybinding",
            true,
            "Enables or disables keyboard shortcuts to show or hide windows in Flight and OAB scenes."
        );
            
        Keybind1 = Plugin.Config.Bind(
            "Keybinding",
            "Keycode 1",
            KeyCode.LeftControl,
            "First keycode."
        );
            
        Keybind2 = Plugin.Config.Bind(
            "Keybinding",
            "Keycode 2",
            KeyCode.E,
            "Second keycode.");
            
        MainUpdateLoopUpdateFrequency = Plugin.Config.Bind(
            "Update Frequency (ms)",
            "Main Update",
            100,
            new ConfigDescription(
                "Time in milliseconds between every entry refresh.\n\nIncrease the value for better performance at the cost of longer time between updates.", 
                new AcceptableValueRange<int>(0, 1000))
        );
            
        StageInfoUpdateFrequency = Plugin.Config.Bind(
            "Update Frequency (ms)",
            "Stage Info",
            500,
            new ConfigDescription(
                "Time in milliseconds between every Stage Info refresh.\n\nIncrease the value for better performance at the cost of longer time between updates.",
                new AcceptableValueRange<int>(MainUpdateLoopUpdateFrequency.Value, 1000))
        );
    }
}