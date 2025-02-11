﻿using System.Reflection;
using KSP.Game;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using Newtonsoft.Json;
using UnityEngine;
using static KSP.Rendering.Planets.PQSData;
using ReduxLib.Logging;
using JetBrains.Annotations;
using KSP.Game.Science;
using KSP.Messages;
using KSP.Sim.DeltaV;
using MicroEngineer.Managers;
using MicroEngineer.Windows;
using UitkForKsp2.API;
using UnityEngine.UIElements;
using ILogger = ReduxLib.Logging.ILogger;

namespace MicroEngineer.Utilities;

public static class Utility
{
    public static VesselComponent ActiveVessel;
    public static ManeuverNodeData CurrentManeuver;
    public static string LayoutPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MicroLayout.json");
    public static int CurrentLayoutVersion = 14;
    private static ILogger Logger = ReduxLib.ReduxLib.GetLogger("MicroEngineer.Utility");
    public static GameStateConfiguration GameState;
    public static MessageCenter MessageCenter;
    public static VesselDeltaVComponent VesselDeltaVComponentOAB;
    
    public static double UniversalTime => GameManager.Instance.Game.UniverseModel.UniverseTime;
    public static string Body => ActiveVessel.mainBody.bodyName;
    public static byte PlayerId => GameManager.Instance.Game.LocalPlayer.PlayerId;

    private static Dictionary<string, CelestialBodyScienceRegionsData> _scienceRegions;
    public static Dictionary<string, CelestialBodyScienceRegionsData> ScienceRegions
    {
        get
        {
            if (_scienceRegions != null)
                return _scienceRegions;
            
            _scienceRegions = GetScienceRegions();
            return _scienceRegions;
        }
    }

    /// <summary>
    /// Refreshes the ActiveVessel and CurrentManeuver
    /// </summary>
    public static void RefreshActiveVesselAndCurrentManeuver()
    {
        ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
        CurrentManeuver = ActiveVessel != null ? GameManager.Instance?.Game?.SpaceSimulation.Maneuvers.GetNodesForVessel(ActiveVessel.GlobalId).FirstOrDefault() : null;
    }

    public static void RefreshGameManager()
    {
        GameState = GameManager.Instance?.Game?.GlobalGameState?.GetGameState();
        MessageCenter = GameManager.Instance?.Game?.Messages;
    }

    public static void RefreshStagesOAB()
    {
        VesselDeltaVComponentOAB = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.VesselDeltaV;
    }

    public static LatLonParsed ParseDegreesToDMSFormat(double inputDegree)
    {
        var ts = TimeSpan.FromHours(Math.Abs(inputDegree));
        int degrees = (int)Math.Floor(ts.TotalHours);
        int minutes = ts.Minutes;
        int seconds = ts.Seconds;

        return new LatLonParsed () { Degrees = degrees, Minutes = minutes, Seconds = seconds };
    }

    public static TimeParsed ParseSecondsToTimeFormat(double inputSeconds)
    {
        if (inputSeconds == double.PositiveInfinity)
            return TimeParsed.MaxValue();
        else if (inputSeconds == double.NegativeInfinity)
            return TimeParsed.MinValue();

        inputSeconds = Math.Ceiling(inputSeconds);
        var absoluteSeconds = Math.Abs(inputSeconds);

        int years = (int)(absoluteSeconds / 9201600); // 426 days in a year
        int days = (int)((absoluteSeconds - years * 9201600) / 21600); // 6 hours in a day
        int hours = (int)((absoluteSeconds - years * 9201600 - days * 21600) / 3600);
        int minutes = (int)((absoluteSeconds - years * 9201600 - days * 21600 - hours * 3600 ) / 60);
        int seconds = (int)(absoluteSeconds - years * 9201600 - days * 21600 - hours * 3600 - minutes * 60);

        // If inputSeconds is negative, reverse the sign of the highest calculated value
        if (inputSeconds < 0)
        {
            if (years != 0) years = -years;
            else if (days != 0) days = -days;
            else if (hours != 0) hours = -hours;
            else if (minutes != 0) minutes = -minutes;
            else if (seconds != 0) seconds = -seconds;
        }

        return new TimeParsed() { Years = years, Days = days, Hours = hours, Minutes = minutes, Seconds = seconds };
    }

    public static string SituationToString(VesselSituations situation)
    {
        return situation switch
        {
            VesselSituations.PreLaunch => "Pre-Launch",
            VesselSituations.Landed => "Landed",
            VesselSituations.Splashed => "Splashed down",
            VesselSituations.Flying => "Flying",
            VesselSituations.SubOrbital => "Suborbital",
            VesselSituations.Orbiting => "Orbiting",
            VesselSituations.Escaping => "Escaping",
            _ => "UNKNOWN",
        };
    }

    public static string BiomeToString(BiomeSurfaceData biome)
    {
        string result = biome.type.ToString().ToLower().Replace('_', ' ');
        return result.Substring(0, 1).ToUpper() + result.Substring(1);
    }
    
    public static string ScienceSituationToString(ScienceSitutation situation)
    {
        return situation switch
        {
            ScienceSitutation.None => "None",
            ScienceSitutation.HighOrbit => "High Orbit",
            ScienceSitutation.LowOrbit => "Low Orbit",
            ScienceSitutation.Atmosphere => "Atmosphere",
            ScienceSitutation.Splashed => "Splashed",
            ScienceSitutation.Landed => "Landed",
            _ => "UNKNOWN",
        };
    }

    public static string ScienceRegionToString(string region, string body)
    {
        // remove "body" instance from "region"
        if (region.Contains(body, StringComparison.OrdinalIgnoreCase))
            region = region.Replace(body, string.Empty, StringComparison.OrdinalIgnoreCase);

        return region;
    }
    
    public static string ExperimentStateToString(ScienceActionGroupState experimentState)
    {
        return experimentState switch
        {
            ScienceActionGroupState.None => "Nothing new", // 0
            ScienceActionGroupState.UnScoredAvailable => "Available!", // 1
            ScienceActionGroupState.UnScoredEvaAvailable => "EVA Available!", // 2
            (ScienceActionGroupState)3 => "Available!", // 1 & 2
            ScienceActionGroupState.ExperimentsInProgress => "In progress", // 4
            _ => "UNKNOWN",
        };
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * PatchedConicsOrbit.Rad2Deg;
    }

    public static void SaveLayout()
    {
        try
        {
            File.WriteAllText(LayoutPath, JsonConvert.SerializeObject(Manager.Instance.Windows));
            Logger.LogDebug("SaveLayout successful");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error trying to SaveLayout. Error description: " + ex);
        }
    }

    public static void LoadLayout(List<BaseWindow> windows)
    {
        try
        {
            List<BaseWindow> deserializedWindows = JsonConvert.DeserializeObject<List<BaseWindow>>(File.ReadAllText(LayoutPath));

            // Check if user has an old layout version. If it's older, it's not supported, so the default layout will remain active. Once the new layout is saved, it will persist.
            var MainGui = deserializedWindows.OfType<MainGuiWindow>().FirstOrDefault();
            if (MainGui.LayoutVersion < Utility.CurrentLayoutVersion)
            {
                Logger.LogInfo("Loaded layout version is older than the current supported version. Layout will be reset.");
                return;
            }

            windows.Clear();
            windows.AddRange(deserializedWindows);

            var settingsWindow = windows.Find(w => w.GetType() == typeof(SettingsWindow)) as SettingsWindow;
            settingsWindow.LoadSettings();

            Logger.LogDebug("LoadLayout successful.");
        }
        catch (FileNotFoundException)
        {
            Logger.LogWarning($"MicroLayout.json file was not found at the expected location during LoadLayout. This is normal if this mod was just installed. Window states and positions will keep their default values.");

        }
        catch (Exception ex)
        {
            Logger.LogError("Error trying to LoadLayout. Full error description:\n" + ex);
        }
    }

    /// <summary>
    /// Check if current vessel has an active target (celestial body or vessel)
    /// </summary>
    /// <returns></returns>
    public static bool TargetExists()
    {
        try { return ActiveVessel.TargetObject != null; }
        catch { return false; }
    }

    /// <summary>
    /// Checks if current vessel has a maneuver
    /// </summary>
    /// <returns></returns>
    public static bool ManeuverExists()
    {
        try { return GameManager.Instance?.Game?.SpaceSimulation.Maneuvers.GetNodesForVessel(ActiveVessel.GlobalId).FirstOrDefault() != null; }
        catch { return false; }
    }

    public static void ClampToScreenUitk(VisualElement root)
    {
        var position = root.transform.position;
        var size = new Vector2(root.worldBound.width, root.worldBound.height);

        float x = Mathf.Clamp(position.x, 0, ReferenceResolution.Width - size.x);
        float y = Mathf.Clamp(position.y, 0, ReferenceResolution.Height - size.y);
        root.transform.position = new Vector2(x, size.y > ReferenceResolution.Height ? position.y : y);
    }

    public static bool AreRectsNear(Rect rect1, Rect rect2)
    {
        float distanceX = Mathf.Abs(rect1.center.x - rect2.center.x);
        float distanceY = Mathf.Abs(rect1.center.y - rect2.center.y);
        return (distanceX < rect1.width / 2 + rect2.width / 2 + 50f && distanceY < rect1.height / 2 + rect2.height / 2 + 50f);
    }

    /// <summary>
    /// Centered UITK window on GeometryChangedEvent
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="element">Root element for which width and height will be taken</param>
    public static void CenterWindow(GeometryChangedEvent evt, VisualElement element)
    {
        if (evt.newRect.width == 0 || evt.newRect.height == 0)
            return;

        element.transform.position = new Vector2((ReferenceResolution.Width - evt.newRect.width) / 2, (ReferenceResolution.Height - evt.newRect.height) / 2);
        element.UnregisterCallback<GeometryChangedEvent>((evt) => CenterWindow(evt, element));
    }

    [CanBeNull]
    public static Dictionary<string, CelestialBodyScienceRegionsData> GetScienceRegions()
    {
        var scienceRegionsProvider = GameManager.Instance.Game.ScienceManager?.ScienceRegionsDataProvider; //._cbToScienceRegions

        if (scienceRegionsProvider == null)
            return null;

        Type providerType = scienceRegionsProvider.GetType();

        FieldInfo cbToScienceRegionsField =
            providerType.GetField("_cbToScienceRegions", BindingFlags.NonPublic | BindingFlags.Instance);

        if (cbToScienceRegionsField == null)
            return null;

        var scienceRegions =
            cbToScienceRegionsField.GetValue(scienceRegionsProvider) as
                Dictionary<string, CelestialBodyScienceRegionsData>;

        return scienceRegions;
        // _cbToScienceRegions	Count = 17	System.Collections.Generic.Dictionary<string, KSP.Game.Science.CelestialBodyScienceRegionsData>
    }

    public static CelestialBodyScienceRegionsData GetBodyScienceRegion(string body)
    {
        return !ScienceRegions.ContainsKey(body) ? null : ScienceRegions[body];
    }

    /*
    public static void DisableGameInputOnFocus(this VisualElement element)
    {
        element.RegisterCallback<FocusInEvent>(_ => GameManager.Instance?.Game?.Input.Disable());
        element.RegisterCallback<FocusOutEvent>(_ => GameManager.Instance?.Game?.Input.Enable());
    }
    */
}