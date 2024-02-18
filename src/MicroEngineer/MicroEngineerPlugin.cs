using System.Reflection;
using BepInEx;
using JetBrains.Annotations;
using KSP.Game;
using MicroEngineer.Managers;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using MicroEngineer.UI;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using Utility = MicroEngineer.Utilities.Utility;

namespace MicroEngineer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class MicroEngineerPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static MicroEngineerPlugin Instance { get; set; }

    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-MicroEngineerFlight";
    internal const string ToolbarOabButtonID = "BTN-MicroEngineerOAB";
    internal const string ToolbarKscButtonID = "BTN-MicroEngineerKSC";
    
    public Coroutine MainUpdateLoop;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        // Load all the other assemblies used by this mod
        LoadAssemblies();
        
        MessageManager.Instance.SubscribeToMessages();

        /*
        // Load the UI from the asset bundle
        var myFirstWindowUxml = AssetManager.GetAsset<VisualTreeAsset>(
            // The case-insensitive path to the asset in the bundle is composed of:
            // - The mod GUID:
            $"{ModGuid}/" +
            // - The name of the asset bundle:
            "MicroEngineer_ui/" +
            // - The path to the asset in your Unity project (without the "Assets/" part)
            "ui/myfirstwindow/myfirstwindow.uxml"
        );
        */

        /*
        // Create the window options object
        var windowOptions = new WindowOptions
        {
            // The ID of the window. It should be unique to your mod.
            WindowId = "MicroEngineer_MyFirstWindow",
            // The transform of parent game object of the window.
            // If null, it will be created under the main canvas.
            Parent = null,
            // Whether or not the window can be hidden with F2.
            IsHidingEnabled = true,
            // Whether to disable game input when typing into text fields.
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions
            {
                // Whether or not the window can be moved by dragging.
                IsMovingEnabled = true,
                // Whether or not the window can only be moved within the screen bounds.
                CheckScreenBounds = true
            }
        };
        */

        /*
        // Create the window
        var myFirstWindow = Window.Create(windowOptions, myFirstWindowUxml);
        // Add a controller for the UI to the window's game object
        var myFirstWindowController = myFirstWindow.gameObject.AddComponent<MyFirstWindowController>();
        */

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen =>
            {
                if (isOpen)
                {
                    var mainWindow = Manager.Instance.Windows.Find(w => w is MainGuiWindow) as MainGuiWindow;
                    mainWindow.IsFlightActive = true;
                    mainWindow.IsFlightMinimized = false;
                }
                FlightSceneController.Instance.ShowGui = isOpen;
                Utility.SaveLayout();
            }
        );

        // Register OAB AppBar Button
        Appbar.RegisterOABAppButton(
            ModName,
            ToolbarOabButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen =>
            {
                if (isOpen)
                    Manager.Instance.Windows.Find(w => w.GetType() == typeof(StageInfoOabWindow)).IsEditorActive = isOpen;
                OABSceneController.Instance.ShowGui = isOpen;
                Utility.SaveLayout();
            }
        );
        
        Settings.Initialize();
        
        MainUpdateLoop = StartCoroutine(DoFlightUpdate());
    }

    /// <summary>
    /// Loads all the assemblies for the mod.
    /// </summary>
    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "MicroEngineer.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        CustomControls.RegisterFromAssembly(unityAssembly);
    }
    
    private System.Collections.IEnumerator DoFlightUpdate()
    {
        while (true)
        {
            try
            {
                Manager.Instance.DoFlightUpdate();
            }
            catch (Exception ex)
            {
                Logger.LogError("Unhandled exception in the DoFlightUpdate loop.\n" +
                                $"Exception: {ex.Message}\n" +
                                $"Stack Trace: {ex.StackTrace}");
            }
                 
            yield return new WaitForSeconds((float)Settings.MainUpdateLoopUpdateFrequency.Value / 1000);
        }
    }
    
    public void Update()
    {
        // Keyboard shortcut for opening the UI
        if ((Settings.EnableKeybinding?.Value ?? false) &&
            (Settings.Keybind1.Value != KeyCode.None ? Input.GetKey(Settings.Keybind1.Value) : true) &&
            (Settings.Keybind2.Value != KeyCode.None ? Input.GetKeyDown(Settings.Keybind2.Value) : true))
        {
            if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView)
            {
                var mainWindow = Manager.Instance.Windows.Find(w => w is MainGuiWindow) as MainGuiWindow;

                // if MainGUI is minimized then treat it like it isn't open at all
                if (mainWindow.IsFlightMinimized)
                {
                    mainWindow.IsFlightMinimized = false;
                    mainWindow.IsFlightActive = true;
                    FlightSceneController.Instance.ShowGui = true;                        
                }
                else
                {
                    bool guiState = FlightSceneController.Instance.ShowGui;
                    FlightSceneController.Instance.ShowGui = !guiState;
                    mainWindow.IsFlightActive = !guiState;
                }
                Utility.SaveLayout();
            }
            else if (Utility.GameState.GameState == GameState.VehicleAssemblyBuilder)
            {
                bool guiState = OABSceneController.Instance.ShowGui;
                OABSceneController.Instance.ShowGui = !guiState;
                Manager.Instance.Windows.Find(w => w.GetType() == typeof(StageInfoOabWindow)).IsEditorActive = !guiState;
                Utility.SaveLayout();
            }
        }
    }
}
