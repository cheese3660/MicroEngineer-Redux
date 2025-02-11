﻿using KSP.Game;
using System.Reflection;
using MicroEngineer.Entries;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UitkForKsp2.API;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace MicroEngineer.Managers;

public class Manager
{
    private static Manager _instance;

    public List<BaseWindow> Windows;
    public List<BaseEntry> Entries;

    private static readonly ILogger _logger = ReduxLib.ReduxLib.GetLogger("MicroEngineer.Manager");
    
    private DateTime _timeOfLastStageInfoUpdate = DateTime.Now;

    public Manager()
    {
        Entries = InitializeEntries();
        Windows = InitializeWindows();
    }

    public static Manager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Manager();

            return _instance;
        }
    }

    public void DoFlightUpdate()
    {
        Utility.RefreshGameManager();

        bool isFlightActive = Windows.OfType<MainGuiWindow>().FirstOrDefault().IsFlightActive;

        // Perform flight UI updates only if we're in Flight or Map view
        if (Utility.GameState != null && (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView) && isFlightActive)
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();

            if (Utility.ActiveVessel == null)
                return;

            // Refresh all active windows' entries
            foreach (EntryWindow window in Windows.Where(w => w.IsFlightActive && w is EntryWindow))
            {
                // StageWindow will have a slower refresh rate as it impacts performance greatly
                if (window is StageWindow )
                {
                    var now = DateTime.Now;
                    TimeSpan elapsedTime = DateTime.Now - _timeOfLastStageInfoUpdate;
                    if (elapsedTime.TotalSeconds > (float)Settings.StageInfoUpdateFrequency.Value / 1000)
                    {
                        _timeOfLastStageInfoUpdate = DateTime.Now;
                    }
                    else
                    {
                        continue;
                    }
                }
                
                window.RefreshData();
            }
        }
    }

    /// <summary>
    /// Builds the list of all Entries
    /// </summary>
    public List<BaseEntry> InitializeEntries()
    {
        Entries = new List<BaseEntry>();

        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] types = assembly.GetTypes();

        // Exclude base classes
        Type[] excludedTypes = new[] { typeof(BaseEntry), typeof(BodyEntry), typeof(FlightEntry),
            typeof(ManeuverEntry), typeof(MiscEntry), typeof(OabStageInfoEntry), typeof(OrbitalEntry),
            typeof(StageInfoEntry), typeof(SurfaceEntry), typeof(TargetEntry), typeof(VesselEntry) };

        Type[] entryTypes = types.Where(t => typeof(BaseEntry).IsAssignableFrom(t) && !excludedTypes.Contains(t)).ToArray();

        foreach (Type entryType in entryTypes)
        {
            BaseEntry entry = Activator.CreateInstance(entryType) as BaseEntry;
            if (entry != null)
                Entries.Add(entry);
        }

        return Entries;
    }

    /// <summary>
    /// Builds the default Windows and fills them with default Entries
    /// </summary>
    public List<BaseWindow> InitializeWindows()
    {
        Windows = new List<BaseWindow>();

        try
        {
            Windows.Add(new MainGuiWindow
            {
                LayoutVersion = Utility.CurrentLayoutVersion,
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                //EditorRect = null,
                FlightRect = new Rect(1350, 160, 0, 0) // About 3/4 of the screen
            });

            Windows.Add(new SettingsWindow
            {
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width/2, ReferenceResolution.Height/2, 0, 0)
            });

            Windows.Add(new EntryWindow
            {
                Name = "Vessel",
                Abbreviation = "VES",
                Description = "Vessel entries",
                IsEditorActive = false,
                IsFlightActive = true,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Vessel,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Vessel && entry.IsDefault).ToList()
            });

            Windows.Add(new EntryWindow
            {
                Name = "Orbital",
                Abbreviation = "ORB",
                Description = "Orbital entries",
                IsEditorActive = false,
                IsFlightActive = true,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Orbital,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Orbital && entry.IsDefault).ToList()
            });

            Windows.Add(new EntryWindow
            {
                Name = "Surface",
                Abbreviation = "SUR",
                Description = "Surface entries",
                IsEditorActive = false,
                IsFlightActive = true,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Surface,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Surface && entry.IsDefault).ToList()
            });

            Windows.Add(new EntryWindow
            {
                Name = "Flight",
                Abbreviation = "FLT",
                Description = "Flight entries",
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Flight,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Flight && entry.IsDefault).ToList()
            });

            Windows.Add(new TargetWindow
            {
                Name = "Target",
                Abbreviation = "TGT",
                Description = "Flight entries",
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Target,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Target && entry.IsDefault).ToList()
            });

            Windows.Add(new ManeuverWindow
            {
                Name = "Maneuver",
                Abbreviation = "MAN",
                Description = "Maneuver entries",
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Maneuver,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Maneuver && entry.IsDefault).ToList()
            });

            Windows.Add(new StageWindow
            {
                Name = "Stage",
                Abbreviation = "STG",
                Description = "Stage entries",
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.Stage,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.Stage && entry.IsDefault).ToList()
            });

            Windows.Add(new StageInfoOabWindow
            {
                IsEditorActive = true,
                IsFlightActive = false, // Not used
                IsMapActive = false, // Not used
                EditorRect = new Rect(645, 41, 0, 0), // Top-center of the screen
                Entries = Entries.Where(entry => entry.Category == MicroEntryCategory.OAB && entry.IsDefault).ToList()
            });
            
            Windows.Add(new EntryWindow
            {
                Name = "For Science!",
                Abbreviation = "SCI",
                Description = "For Science entries",
                IsEditorActive = false,
                IsFlightActive = false,
                IsMapActive = false,
                IsEditorPoppedOut = false,
                IsFlightPoppedOut = false,
                IsMapPoppedOut = false,
                IsLocked = false,
                MainWindow = MainWindow.None,
                //EditorRect = null,
                FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
                Entries = new()
                {
                    Entries.Find(e => e is Body),
                    Entries.Find(e => e is ScienceSituationEntry),
                    Entries.Find(e => e is ScienceRegion),
                    Entries.Find(e => e is ScienceExperimentState),
                    Entries.Find(e => e is AvailableSciencePoints),
                    Entries.Find(e => e is BodyAtmosphereMaxAltitude),
                    Entries.Find(e => e is BodyLowOrbitMaxAltitude),
                    Entries.Find(e => e is BodyHighOrbitMaxAltitude)
                }
            });

            return Windows;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating a BaseWindow. Full exception: " + ex);
            return null;
        }
    }

    /// <summary>
    /// Creates a new custom window user can fill with any entry
    /// </summary>
    /// <param name="editableWindows"></param>
    public int CreateCustomWindow(List<EntryWindow> editableWindows)
    {
        // Default window's name will be CustomX where X represents the first not used integer
        int nameID = 1;
        foreach (EntryWindow window in editableWindows)
        {
            if (window.Name == "Custom" + nameID)
                nameID++;
        }

        EntryWindow newWindow = new()
        {
            Name = "Custom" + nameID,
            Abbreviation = nameID.ToString().Length == 1 ? "Cu" + nameID : nameID.ToString().Length == 2 ? "C" + nameID : nameID.ToString(),
            Description = "",
            IsEditorActive = false,
            IsFlightActive = true,
            IsMapActive = false,
            IsEditorPoppedOut = false,
            IsFlightPoppedOut = false,
            IsMapPoppedOut = false,
            IsLocked = false,
            MainWindow = MainWindow.None,
            //EditorRect = null,
            FlightRect = new Rect(ReferenceResolution.Width / 2, ReferenceResolution.Height / 2, 0, 0),
            Entries = new List<BaseEntry>()
        };

        Windows.Add(newWindow);
        editableWindows.Add(newWindow);

        return editableWindows.Count - 1;
    }

    /// <summary>
    /// TODO implement layout reset
    /// </summary>
    public void ResetLayout()
    {
        Windows.Clear();
        Entries.Clear();
        Entries = InitializeEntries();
        Windows = InitializeWindows();
    }
}