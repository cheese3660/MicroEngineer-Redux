﻿using BepInEx.Logging;
using KSP.Game;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.UI.Binding;
using UnityEngine;

namespace MicroMod
{
    internal class UI
    {
        private MicroEngineerMod _plugin;
        private Manager _manager;

        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("MicroEngineer.MessageManager");

        private List<BaseWindow> _windows;

        internal bool ShowGuiFlight;
        internal bool ShowGuiOAB;

        // If game input is enabled or disabled (used for locking controls when user is editing a text field
        private bool _gameInputState = true;

        #region Editing window
        private bool _showEditWindow = false;
        private int _selectedWindowId = 0;
        private MicroEntryCategory _selectedCategory = MicroEntryCategory.Vessel;
        private (bool condition, int index) _showTooltip = (false, 0);
        #endregion

        // Index of the stage for which user wants to select a different CelestialBody for different TWR calculations. -1 -> no stage is selected
        internal int CelestialBodySelectionStageIndex = -1;
        private bool _showGuiSettingsOAB;

        /// <summary>
        /// Holds data on all bodies for calculating TWR (currently)
        /// </summary>
        internal MicroCelestialBodies CelestialBodies = new();

        internal UI(MicroEngineerMod plugin, Manager manager)
        {
            _plugin = plugin;
            _manager = manager;
            _windows = manager.Windows;
        }

        internal void OnGUI()
        {
            GUI.skin = Styles.SpaceWarpUISkin;

            Utility.RefreshGameManager();
            if (Utility.GameState?.GameState == GameState.VehicleAssemblyBuilder)
                OnGUI_OAB();
            else
                OnGUI_Flight();
        }

        private void OnGUI_Flight()
        {
            _gameInputState = Utility.ToggleGameInputOnControlInFocus(_gameInputState, ShowGuiFlight);

            if (!ShowGuiFlight || Utility.ActiveVessel == null) return;

            BaseWindow mainGui = _windows.Find(window => window.MainWindow == MainWindow.MainGui);

            // Draw main GUI that contains docked windows
            mainGui.FlightRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                mainGui.FlightRect,
                FillMainGUI,
                "<color=#696DFF>// MICRO ENGINEER</color>",
                Styles.MainWindowStyle,
                GUILayout.Height(0)
            );
            mainGui.FlightRect.position = Utility.ClampToScreen(mainGui.FlightRect.position, mainGui.FlightRect.size);

            // Draw all other popped out windows
            foreach (var (window, index) in _windows
                .Select((window, index) => (window, index))
                .Where(x => x.window.IsFlightActive && x.window.IsFlightPoppedOut) // must be active & popped out
                .Where(x => x.window.MainWindow != MainWindow.Settings && x.window.MainWindow != MainWindow.Stage && x.window.MainWindow != MainWindow.MainGui)) // MainGUI, Settings and Stage are special, they'll be drawn separately
            {
                // Skip drawing of Target window if there's no active target
                if (window.MainWindow == MainWindow.Target && !Utility.TargetExists())
                    continue;

                // Skip drawing of Maneuver window if there's no active maneuver
                if (window.MainWindow == MainWindow.Maneuver && !Utility.ManeuverExists())
                    continue;

                // If window is locked set alpha to 80%
                if (window.IsLocked)
                    GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.8f);

                window.FlightRect = GUILayout.Window(
                    index,
                    window.FlightRect,
                    DrawPopoutWindow,
                    "",
                    Styles.PopoutWindowStyle,
                    GUILayout.Height(0),
                    GUILayout.Width(Styles.WindowWidth
                    ));

                // Set alpha back to 100%
                if (window.IsLocked)
                    GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);

                window.FlightRect.position = Utility.ClampToScreen(window.FlightRect.position, window.FlightRect.size);
            }

            // Draw popped out Settings
            int settingsIndex = _windows.FindIndex(window => window.MainWindow == MainWindow.Settings);
            if (_windows[settingsIndex].IsFlightActive && _windows[settingsIndex].IsFlightPoppedOut)
            {
                _windows[settingsIndex].FlightRect = GUILayout.Window(
                    settingsIndex,
                    _windows[settingsIndex].FlightRect,
                    DrawSettingsWindow,
                    "",
                    Styles.PopoutWindowStyle,
                    GUILayout.Height(0),
                    GUILayout.Width(Styles.WindowWidth)
                    );

                _windows[settingsIndex].FlightRect.position = Utility.ClampToScreen(_windows[settingsIndex].FlightRect.position, _windows[settingsIndex].FlightRect.size);
            }

            // Draw popped out Stages
            int stageIndex = _windows.FindIndex(window => window.MainWindow == MainWindow.Stage);
            if (_windows[stageIndex].IsFlightActive && _windows[stageIndex].IsFlightPoppedOut)
            {
                _windows[stageIndex].FlightRect = GUILayout.Window(
                    stageIndex,
                    _windows[stageIndex].FlightRect,
                    DrawStages,
                    "",
                    Styles.PopoutWindowStyle,
                    GUILayout.Height(0),
                    GUILayout.Width(Styles.WindowWidth)
                    );

                _windows[stageIndex].FlightRect.position = Utility.ClampToScreen(_windows[stageIndex].FlightRect.position, _windows[stageIndex].FlightRect.size);
            }

            // Draw Edit Window
            if (_showEditWindow)
            {
                Styles.EditWindowRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    Styles.EditWindowRect,
                    DrawEditWindow,
                    "",
                    Styles.EditWindowStyle,
                    GUILayout.Height(0)
                    );
            }
        }

        private void OnGUI_OAB()
        {
            if (!ShowGuiOAB) return;

            BaseWindow stageInfoOAB = _windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB);
            if (stageInfoOAB.Entries.Find(e => e.Name == "Stage Info (OAB)").EntryValue == null) return;

            stageInfoOAB.EditorRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                stageInfoOAB.EditorRect,
                DrawStageInfoOAB,
                "",
                Styles.StageOABWindowStyle,
                GUILayout.Height(0)
                );
            stageInfoOAB.EditorRect.position = Utility.ClampToScreen(stageInfoOAB.EditorRect.position, stageInfoOAB.EditorRect.size);

            // Draw window for selecting CelestialBody for a stage
            // -1 -> no selection of CelestialBody is taking place
            // any other int -> index represents the stage number for which the selection was clicked
            if (CelestialBodySelectionStageIndex > -1)
            {
                Rect stageInfoOabRect = _windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB).EditorRect;
                Rect celestialBodyRect = new Rect(stageInfoOabRect.x + stageInfoOabRect.width, stageInfoOabRect.y, 0, 0);

                celestialBodyRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    celestialBodyRect,
                    DrawCelestialBodySelection,
                    "",
                    Styles.CelestialSelectionStyle,
                    GUILayout.Height(0)
                    );
            }

            // Draw Settings window for the StageInfoOAB
            if (_showGuiSettingsOAB)
            {
                Rect stageInfoOabRect = _windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB).EditorRect;
                Rect settingsRect = new Rect(stageInfoOabRect.x + stageInfoOabRect.width, stageInfoOabRect.y, 0, 0);

                settingsRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    settingsRect,
                    DrawSettingsOabWindow,
                    "",
                    Styles.SettingsOabStyle,
                    GUILayout.Height(0)
                    );
            }
        }

        #region Flight scene UI
        /// <summary>
        /// Draws the main GUI with all windows that are toggled and docked
        /// </summary>
        /// <param name="windowID"></param>
        private void FillMainGUI(int windowID)
        {
            try
            {
                if (CloseButton(Styles.CloseBtnRect))
                {
                    CloseWindow();
                }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();

                int toggleIndex = -1;
                // Draw toggles for all windows except MainGui and StageInfoOAB
                foreach (BaseWindow window in _windows.Where(x => x.MainWindow != MainWindow.MainGui && x.MainWindow != MainWindow.StageInfoOAB))
                {
                    // layout can fit 6 toggles, so if all 6 slots are filled then go to a new line. Index == 0 is the MainGUI which isn't rendered
                    if (++toggleIndex % 6 == 0 && toggleIndex > 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                    window.IsFlightActive = GUILayout.Toggle(window.IsFlightActive, window.Abbreviation, Styles.SectionToggleStyle);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                // Draw Settings window first
                int settingsIndex = _windows.FindIndex(window => window.MainWindow == MainWindow.Settings);
                if (_windows[settingsIndex].IsFlightActive && !_windows[settingsIndex].IsFlightPoppedOut)
                    DrawSettingsWindow(settingsIndex);

                // Draw Stage window next
                int stageIndex = _windows.FindIndex(window => window.MainWindow == MainWindow.Stage);
                if (_windows[stageIndex].IsFlightActive && !_windows[stageIndex].IsFlightPoppedOut)
                    DrawStages(stageIndex);

                // Draw all other windows
                foreach (var (window, index) in _windows
                    .Select((window, index) => (window, index))
                    .Where(x => x.window.IsFlightActive && !x.window.IsFlightPoppedOut) // must be active & docked
                    .Where(x => x.window.MainWindow != MainWindow.Settings && x.window.MainWindow != MainWindow.Stage && x.window.MainWindow != MainWindow.MainGui)) // MainGUI, Settings and Stage are special, they'll be drawn separately

                {
                    // Skip drawing of Target window if there's no active target
                    if (window.MainWindow == MainWindow.Target && !Utility.TargetExists())
                        continue;

                    // Skip drawing of Maneuver window if there's no active maneuver
                    if (window.MainWindow == MainWindow.Maneuver && !Utility.ManeuverExists())
                        continue;

                    DrawSectionHeader(window.Name, ref window.IsFlightPoppedOut, window.IsLocked, "");

                    window.DrawWindowHeader();

                    foreach (BaseEntry entry in window.Entries)
                    {
                        if (entry.HideWhenNoData && entry.ValueDisplay == "-")
                            continue;
                        DrawEntry(entry.Name, entry.ValueDisplay, entry.Unit);
                    }


                    window.DrawWindowFooter();

                    DrawSectionEnd(window);
                }

                GUI.DragWindow(new Rect(0, 0, Styles.WindowWidth, Styles.WindowHeight));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        /// <summary>
        /// Draws all windows that are toggled and popped out
        /// </summary>
        /// <param name="windowIndex"></param>
        private void DrawPopoutWindow(int windowIndex)
        {
            BaseWindow windowToDraw = _windows[windowIndex];

            DrawSectionHeader(windowToDraw.Name, ref windowToDraw.IsFlightPoppedOut, windowToDraw.IsLocked, "");

            windowToDraw.DrawWindowHeader();

            foreach (BaseEntry entry in windowToDraw.Entries)
            {
                if (entry.HideWhenNoData && entry.ValueDisplay == "-")
                    continue;
                DrawEntry(entry.Name, entry.ValueDisplay, entry.Unit);
            }

            windowToDraw.DrawWindowFooter();

            DrawSectionEnd(windowToDraw);
        }
        
        private void DrawSettingsWindow(int windowIndex)
        {
            BaseWindow windowToDraw = _windows[windowIndex];

            DrawSectionHeader(windowToDraw.Name, ref windowToDraw.IsFlightPoppedOut, windowToDraw.IsLocked, "");

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAVE LAYOUT", Styles.NormalBtnStyle))
                Utility.SaveLayout(_windows);
            GUILayout.Space(5);
            if (GUILayout.Button("LOAD LAYOUT", Styles.NormalBtnStyle))
            {
                Utility.LoadLayout(_windows);
                _manager.Windows = _windows;
            }
                
            GUILayout.Space(5);
            if (GUILayout.Button("RESET", Styles.NormalBtnStyle))
                ResetLayout();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit Windows", Styles.NormalBtnStyle))
            {
                _showEditWindow = !_showEditWindow;
            }
            GUILayout.EndHorizontal();

            DrawSectionEnd(windowToDraw);
        }

        private void DrawStages(int windowIndex)
        {
            BaseWindow windowToDraw = _windows[windowIndex];

            DrawStagesHeader(windowToDraw);

            List<DeltaVStageInfo> stages = (List<DeltaVStageInfo>)windowToDraw.Entries.Find(entry => entry.Name == "Stage Info").EntryValue;

            int stageCount = stages?.Count ?? 0;
            if (stages != null && stageCount > 0)
            {
                float highestTwr = Mathf.Floor(stages.Max(stage => stage.TWRActual));
                int preDecimalDigits = Mathf.FloorToInt(Mathf.Log10(highestTwr)) + 1;
                string twrFormatString = "N2";

                if (preDecimalDigits == 3)
                {
                    twrFormatString = "N1";
                }
                else if (preDecimalDigits == 4)
                {
                    twrFormatString = "N0";
                }

                for (int i = stages.Count - 1; i >= 0; i--)
                {

                    DeltaVStageInfo stageInfo = stages[i];
                    if (stageInfo.DeltaVinVac > 0.0001 || stageInfo.DeltaVatASL > 0.0001)
                    {
                        int stageNum = stageCount - stageInfo.Stage;
                        DrawStageEntry(stageNum, stageInfo, twrFormatString);
                    }
                }
            }

            DrawSectionEnd(windowToDraw);
        }

        private void DrawSectionHeader(string sectionName, ref bool isPopout, bool isLocked, string value = "")
        {
            GUILayout.BeginHorizontal();

            // If window is popped out and it's not locked => show the close button. If it's not popped out => show to popup arrow
            isPopout = isPopout && !isLocked ? !CloseButton(Styles.CloseBtnRect) : !isPopout ? GUILayout.Button("⇖", Styles.PopoutBtnStyle) : isPopout;

            GUILayout.Label($"<b>{sectionName}</b>");
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, Styles.ValueLabelStyle);
            GUILayout.Space(5);
            GUILayout.Label("", Styles.UnitLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(Styles.SpacingAfterHeader);
        }

        private void DrawStagesHeader(BaseWindow stageWindow)
        {
            GUILayout.BeginHorizontal();
            stageWindow.IsFlightPoppedOut = stageWindow.IsFlightPoppedOut ? !CloseButton(Styles.CloseBtnRect) : GUILayout.Button("⇖", Styles.PopoutBtnStyle);

            GUILayout.Label($"<b>{stageWindow.Name}</b>");
            GUILayout.FlexibleSpace();
            GUILayout.Label("∆v", Styles.TableHeaderLabelStyle);
            GUILayout.Space(16);
            GUILayout.Label($"TWR", Styles.TableHeaderLabelStyle, GUILayout.Width(40));
            GUILayout.Space(16);
            if (stageWindow.IsFlightPoppedOut)
            {
                GUILayout.Label($"<color=#{Styles.UnitColorHex}>Burn</color>", GUILayout.Width(56));
            }
            else
            {
                GUILayout.Label($"Burn", Styles.TableHeaderLabelStyle, GUILayout.Width(56));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(Styles.SpacingAfterHeader);
        }

        private void DrawEntry(string entryName, string value, string unit = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(entryName, Styles.NameLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, Styles.ValueLabelStyle);
            GUILayout.Space(5);
            GUILayout.Label(unit, Styles.UnitLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(Styles.SpacingAfterEntry);
        }

        private void DrawStageEntry(int stageID, DeltaVStageInfo stageInfo, string twrFormatString)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{stageID:00.}", Styles.NameLabelStyle, GUILayout.Width(24));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{stageInfo.DeltaVActual:N0} <color=#{Styles.UnitColorHex}>m/s</color>", Styles.ValueLabelStyle);
            GUILayout.Space(16);
            GUILayout.Label($"{stageInfo.TWRActual.ToString(twrFormatString)}", Styles.ValueLabelStyle, GUILayout.Width(40));
            GUILayout.Space(16);
            string burnTime = Utility.SecondsToTimeString(stageInfo.StageBurnTime, false);
            string lastUnit = "s";
            if (burnTime.Contains('h'))
            {
                burnTime = burnTime.Remove(burnTime.LastIndexOf("<color"));
                lastUnit = "m";
            }
            if (burnTime.Contains('d'))
            {
                burnTime = burnTime.Remove(burnTime.LastIndexOf("<color"));
                lastUnit = "h";
            }

            GUILayout.Label($"{burnTime}<color=#{Styles.UnitColorHex}>{lastUnit}</color>", Styles.ValueLabelStyle, GUILayout.Width(56));
            GUILayout.EndHorizontal();
            GUILayout.Space(Styles.SpacingAfterEntry);
        }

        private void DrawSectionEnd(BaseWindow window)
        {
            if (window.IsFlightPoppedOut)
            {
                if (!window.IsLocked)
                    GUI.DragWindow(new Rect(0, 0, Styles.WindowWidth, Styles.WindowHeight));

                GUILayout.Space(Styles.SpacingBelowPopout);
            }
            else
            {
                GUILayout.Space(Styles.SpacingAfterSection);
            }
        }

        /// <summary>
        /// Window for edditing window contents. Add/Remove/Reorder entries.
        /// </summary>
        /// <param name="windowIndex"></param>
        private void DrawEditWindow(int windowIndex)
        {
            List<BaseWindow> editableWindows = _windows.FindAll(w => w.IsEditable); // Editable windows are all except MainGUI, Settings, Stage and StageInfoOAB
            List<BaseEntry> entriesByCategory = _manager.Entries.FindAll(e => e.Category == _selectedCategory); // All window stageInfoOabEntries belong to a category, but they can still be placed in any window

            _showEditWindow = !CloseButton(Styles.CloseBtnRect);

            #region Selection of window to be edited
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>EDITING WINDOW</b>", Styles.TitleLabelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<", Styles.OneCharacterBtnStyle))
            {
                _selectedWindowId = _selectedWindowId > 0 ? _selectedWindowId - 1 : editableWindows.Count - 1;
            }
            GUI.SetNextControlName(Utility.InputDisableWindowAbbreviation);
            editableWindows[_selectedWindowId].Abbreviation = GUILayout.TextField(editableWindows[_selectedWindowId].Abbreviation, Styles.WindowSelectionAbbrevitionTextFieldStyle);
            editableWindows[_selectedWindowId].Abbreviation = Utility.ValidateAbbreviation(editableWindows[_selectedWindowId].Abbreviation);
            GUI.SetNextControlName(Utility.InputDisableWindowName);
            editableWindows[_selectedWindowId].Name = GUILayout.TextField(editableWindows[_selectedWindowId].Name, Styles.WindowSelectionTextFieldStyle);
            if (GUILayout.Button(">", Styles.OneCharacterBtnStyle))
            {
                _selectedWindowId = _selectedWindowId < editableWindows.Count - 1 ? _selectedWindowId + 1 : 0;
            }
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(-10);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            editableWindows[_selectedWindowId].IsLocked = GUILayout.Toggle(editableWindows[_selectedWindowId].IsLocked, "Locked");
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (editableWindows[_selectedWindowId].IsDeletable)
            {
                if (GUILayout.Button("DEL WINDOW", Styles.NormalBtnStyle))
                {
                    _windows.Remove(editableWindows[_selectedWindowId]);
                    editableWindows.Remove(editableWindows[_selectedWindowId]);
                    _selectedWindowId--;
                }
            }
            if (GUILayout.Button("NEW WINDOW", Styles.NormalBtnStyle))
                _selectedWindowId = _manager.CreateCustomWindow(editableWindows);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            Styles.DrawHorizontalLine();
            GUILayout.Space(10);

            #region Installed entries in the selected window
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Installed</b>", Styles.NormalLabelStyle);
            GUILayout.EndHorizontal();

            var entries = editableWindows[_selectedWindowId].Entries.ToList();
            foreach (var (entry, index) in entries.Select((entry, index) => (entry, index)))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Name, Styles.NameLabelStyle);
                if (GUILayout.Button("↑", Styles.OneCharacterBtnStyle))
                {
                    if (index > 0)
                        editableWindows[_selectedWindowId].MoveEntryUp(index);
                }
                if (GUILayout.Button("↓", Styles.OneCharacterBtnStyle))
                {
                    if (index < editableWindows[_selectedWindowId].Entries.Count - 1)
                        editableWindows[_selectedWindowId].MoveEntryDown(index);
                }
                if (GUILayout.Button("X", Styles.OneCharacterBtnStyle))
                    editableWindows[_selectedWindowId].RemoveEntry(index);
                GUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(10);
            Styles.DrawHorizontalLine();
            GUILayout.Space(10);

            #region All entries that can be added to any IsEditable window
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Add</b>", Styles.NormalLabelStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Category", Styles.NormalLabelStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("<", Styles.OneCharacterBtnStyle))
            {
                _selectedCategory = (int)_selectedCategory > 0 ?
                    _selectedCategory - 1 :
                    Enum.GetValues(typeof(MicroEntryCategory)).Cast<MicroEntryCategory>().Last();
            }
            GUILayout.Label(_selectedCategory.ToString(), Styles.NormalCenteredLabelStyle);
            if (GUILayout.Button(">", Styles.OneCharacterBtnStyle))
            {
                _selectedCategory = (int)_selectedCategory < (int)Enum.GetValues(typeof(MicroEntryCategory)).Cast<MicroEntryCategory>().Last() ?
                    _selectedCategory + 1 :
                    Enum.GetValues(typeof(MicroEntryCategory)).Cast<MicroEntryCategory>().First();
            }
            GUILayout.EndHorizontal();

            foreach (var (entry, index) in entriesByCategory.Select((entry, index) => (entry, index)))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Name, Styles.NameLabelStyle);
                if (GUILayout.Button("?", Styles.OneCharacterBtnStyle))
                {
                    if (!_showTooltip.condition)
                        _showTooltip = (true, index);
                    else
                    {
                        if (_showTooltip.index != index)
                            _showTooltip = (true, index);
                        else
                            _showTooltip = (false, index);
                    }
                }
                if (GUILayout.Button("+", Styles.OneCharacterBtnStyle))
                {
                    editableWindows[_selectedWindowId].AddEntry(entry);
                }
                GUILayout.EndHorizontal();

                if (_showTooltip.condition && _showTooltip.index == index)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Description, Styles.BlueLabelStyle);
                    GUILayout.EndHorizontal();
                }
            }
            #endregion

            GUI.DragWindow(new Rect(0, 0, Styles.WindowWidth, Styles.WindowHeight));
        }

        #endregion

        #region OAB scene UI

        private void DrawStageInfoOAB(int windowID)
        {
            BaseWindow stageInfoOabWindow = _windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB);
            List<BaseEntry> stageInfoOabEntries = stageInfoOabWindow.Entries;

            GUILayout.BeginHorizontal();
            if (SettingsButton(Styles.SettingsOABRect))
                _showGuiSettingsOAB = !_showGuiSettingsOAB;

            if (CloseButton(Styles.CloseBtnStagesOABRect))
            {
                stageInfoOabWindow.IsEditorActive = false;
                ShowGuiOAB = false;
            }
            GUILayout.Label($"<b>Stage Info</b>");
            GUILayout.EndHorizontal();

            // Draw StageInfo header - Delta V fields
            GUILayout.BeginHorizontal();
            GUILayout.Label("Total ∆v (ASL, vacuum)", Styles.NameLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{stageInfoOabEntries.Find(e => e.Name == "Total ∆v Actual (OAB)").ValueDisplay}, {stageInfoOabEntries.Find(e => e.Name == "Total ∆v Vac (OAB)").ValueDisplay}", Styles.ValueLabelStyle);
            GUILayout.Space(5);
            GUILayout.Label("m/s", Styles.UnitLabelStyle);
            GUILayout.EndHorizontal();

            // Draw Torque
            Torque torque = (Torque)stageInfoOabEntries.Find(e => e.Name == "Torque");
            if (torque.IsActive)
            {
                GUILayout.Space(Styles.SpacingAfterEntry);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Torque", Styles.NameLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(torque.ValueDisplay, Styles.ValueLabelStyle);
                GUILayout.Space(5);
                GUILayout.Label(torque.Unit, Styles.UnitLabelStyle);
                GUILayout.EndHorizontal();
            }

            // Draw Stage table header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Stage", Styles.NameLabelStyle, GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.Label("TWR", Styles.TableHeaderLabelStyle, GUILayout.Width(65));
            GUILayout.Label("SLT", Styles.TableHeaderLabelStyle, GUILayout.Width(75));
            GUILayout.Label("", Styles.TableHeaderLabelStyle, GUILayout.Width(30));
            GUILayout.Label("ASL ∆v", Styles.TableHeaderLabelStyle, GUILayout.Width(75));
            GUILayout.Label("", Styles.TableHeaderLabelStyle, GUILayout.Width(30));
            GUILayout.Label("Vac ∆v", Styles.TableHeaderLabelStyle, GUILayout.Width(75));
            GUILayout.Label("Burn Time", Styles.TableHeaderLabelStyle, GUILayout.Width(110));
            GUILayout.Space(20);
            GUILayout.Label("Body", Styles.TableHeaderCenteredLabelStyle, GUILayout.Width(80));
            GUILayout.EndHorizontal();
            GUILayout.Space(Styles.SpacingAfterEntry);

            StageInfo_OAB stageInfoOab = (StageInfo_OAB)stageInfoOabWindow.Entries
                .Find(e => e.Name == "Stage Info (OAB)");

            // Draw each stage that has delta v
            var stages = ((List<DeltaVStageInfo_OAB>)stageInfoOab.EntryValue)
                .FindAll(s => s.DeltaVVac > 0.0001 || s.DeltaVASL > 0.0001);

            int celestialIndex = -1;
            for (int stageIndex = stages.Count - 1; stageIndex >= 0; stageIndex--)
            {
                // Check if this stage has a CelestialBody attached. If not, create a new CelestialBody and assign it to HomeWorld (i.e. Kerbin)
                if (stageInfoOab.CelestialBodyForStage.Count == ++celestialIndex)
                    stageInfoOab.AddNewCelestialBody(CelestialBodies);

                GUILayout.BeginHorizontal();
                GUILayout.Label(String.Format("{0:00}", ((List<DeltaVStageInfo_OAB>)stageInfoOab.EntryValue).Count - stages[stageIndex].Stage), Styles.NameLabelStyle, GUILayout.Width(40));
                GUILayout.FlexibleSpace();

                // We calculate what factor needs to be applied to TWR in order to compensate for different gravity of the selected celestial body                
                double twrFactor = CelestialBodies.GetTwrFactor(stageInfoOab.CelestialBodyForStage[celestialIndex]);
                GUILayout.Label(String.Format("{0:N2}", stages[stageIndex].TWRVac * twrFactor), Styles.ValueLabelStyle, GUILayout.Width(65));

                // Calculate Sea Level TWR and DeltaV
                CelestialBodyComponent cel = CelestialBodies.Bodies.Find(b => b.Name == stageInfoOab.CelestialBodyForStage[celestialIndex]).CelestialBodyComponent;
                GUILayout.Label(String.Format("{0:N2}", stages[stageIndex].GetTWRAtSeaLevel(cel) * twrFactor), Styles.ValueLabelStyle, GUILayout.Width(75));
                GUILayout.Label(String.Format("{0:N0}", stages[stageIndex].GetDeltaVelAtSeaLevel(cel)), Styles.ValueLabelStyle, GUILayout.Width(75));
                GUILayout.Label("m/s", Styles.UnitLabelStyleStageOAB, GUILayout.Width(30));

                GUILayout.Label(String.Format("{0:N0}", stages[stageIndex].DeltaVVac), Styles.ValueLabelStyle, GUILayout.Width(75));
                GUILayout.Label("m/s", Styles.UnitLabelStyleStageOAB, GUILayout.Width(30));
                GUILayout.Label(Utility.SecondsToTimeString(stages[stageIndex].StageBurnTime, true, true), Styles.ValueLabelStyle, GUILayout.Width(110));
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(stageInfoOab.CelestialBodyForStage[celestialIndex], Styles.CelestialBodyBtnStyle))
                {
                    CelestialBodySelectionStageIndex = celestialIndex;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(Styles.SpacingAfterEntry);
            }

            GUILayout.Space(Styles.SpacingBelowPopout);

            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        /// <summary>
        /// Opens a window for selecting a CelestialObject for the stage on the given index
        /// </summary>
        private void DrawCelestialBodySelection(int id)
        {
            GUILayout.BeginVertical();

            foreach (var body in CelestialBodies.Bodies)
            {
                if (GUILayout.Button(body.DisplayName, Styles.CelestialSelectionBtnStyle))
                {
                    StageInfo_OAB stageInfoOab = (StageInfo_OAB)_windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB).Entries.Find(e => e.Name == "Stage Info (OAB)");
                    stageInfoOab.CelestialBodyForStage[CelestialBodySelectionStageIndex] = body.Name;

                    // Hide the selection window
                    CelestialBodySelectionStageIndex = -1;
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Opens a Settings window for OAB
        /// </summary>
        private void DrawSettingsOabWindow(int id)
        {
            if (CloseButton(Styles.CloseBtnSettingsOABRect))
                _showGuiSettingsOAB = false;

            BaseWindow stageInfoOabWindow = _windows.Find(w => w.MainWindow == MainWindow.StageInfoOAB);
            List<BaseEntry> stageInfoOabEntries = stageInfoOabWindow.Entries;
            Torque torqueEntry = (Torque)stageInfoOabEntries.Find(e => e.Name == "Torque");

            torqueEntry.IsActive = GUILayout.Toggle(torqueEntry.IsActive, "Display Torque (experimental)\nTurn on CoT & CoM for this", Styles.SectionToggleStyle);
        }

        /// <summary>
        /// Draws a Settings button (≡)
        /// </summary>
        /// <param name="settingsOABRect"></param>
        /// <returns></returns>
        private bool SettingsButton(Rect rect)
        {
            return GUI.Button(rect, "≡", Styles.SettingsBtnStyle);
        }

        #endregion

        private void ResetLayout()
        {
            _windows = _manager.InitializeWindows();
            _selectedWindowId = 0;
        }

        private void CloseWindow()
        {
            GameObject.Find("BTN-MicroEngineerBtn")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
            ShowGuiFlight = false;
        }

        /// <summary>
        /// Draws a close button (X)
        /// </summary>
        /// <param name="rect">Where to position the close button</param>
        /// <returns></returns>
        private bool CloseButton(Rect rect)
        {
            return GUI.Button(rect, "X", Styles.CloseBtnStyle);
        }
    }
}