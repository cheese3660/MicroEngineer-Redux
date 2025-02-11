using System.Reflection;
using JetBrains.Annotations;
using KSP.Game;
using MicroEngineer.Managers;
using SpaceWarp;
using SpaceWarp.API.Mods;
using MicroEngineer.UI;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using Redux.ExtraModTypes;
using SpaceWarp.UI.API.Appbar;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UI;
using Utility = MicroEngineer.Utilities.Utility;

namespace MicroEngineer;

public class MicroEngineerPlugin : KerbalMod
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = "MicroEngineer";
    [PublicAPI] public const string ModName = "MicroEngineer";
    [PublicAPI] public const string ModVer = "1.8.1";

    /// Singleton instance of the plugin class
    [PublicAPI] public static MicroEngineerPlugin Instance { get; set; }

    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-MicroEngineerFlight";
    internal const string ToolbarOabButtonID = "BTN-MicroEngineerOAB";
    internal const string ToolbarKscButtonID = "BTN-MicroEngineerKSC";
    
    public Coroutine MainUpdateLoop;

    internal AssetBundle FlightUi;
    internal AssetBundle OabUi;
    internal Texture2D Icon;
    
    public override void OnPreInitialized()
    {

        var assetsPath = Path.Combine(SWMetadata.Folder.FullName, "assets");
        var bundlesPath = Path.Combine(assetsPath, "bundles");
        var imagesPath = Path.Combine(assetsPath, "images");
        
        var icon = Path.Combine(imagesPath, "icon.png");
        var bytes = File.ReadAllBytes(icon);
        Icon = new Texture2D(2, 2);
        
        Icon.LoadImage(bytes);
        
        
        FlightUi = AssetBundle.LoadFromFile(Path.Combine(bundlesPath, "microengineer_flightui.bundle"));
        OabUi = AssetBundle.LoadFromFile(Path.Combine(bundlesPath, "microengineer_oabui.bundle"));

        foreach (var item in FlightUi.GetAllAssetNames())
        {
            SWLogger.LogInfo($"Flight UI Item: {item}");
        }

        foreach (var item in OabUi.GetAllAssetNames())
        {
            SWLogger.LogInfo($"OAB UI Item: {item}");
        }
        
        Instance = this;
    }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {

        // Load all the other assemblies used by this mod
        LoadAssemblies();
        
        MessageManager.Instance.SubscribeToMessages();
        
        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            Icon,
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
            Icon,
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

    public override void OnPostInitialized()
    {
    }

    /// <summary>
    /// Loads all the assemblies for the mod.
    /// </summary>
    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        var unityAssembly = Assembly.LoadFrom(Path.Combine(Instance.SWMetadata.Folder.FullName, "MicroEngineer.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        if (!Application.isEditor)
        {
            CustomControls.RegisterFromAssembly(unityAssembly);
        }
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
                SWLogger.LogError("Unhandled exception in the DoFlightUpdate loop.\n" +
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
