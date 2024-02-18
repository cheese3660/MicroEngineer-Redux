using MicroEngineer.Entries;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    public class EntryWindowController : MonoBehaviour
    {
        public EntryWindow EntryWindow { get; set; }
        public VisualElement WindowRoot { get; set; }
        public VisualElement Root { get; set; }
        public VisualElement Title { get; set; }
        public VisualElement TitleArrowDown { get; set; }
        public VisualElement TitleArrowRight { get; set; }
        public Label NameLabel { get; set; }
        public Button SettingsButton { get; set; }
        public Button PopOutButton { get; set; }
        public Button CloseButton { get; set; }
        public VisualElement Header { get; set; }
        public VisualElement Body { get; set; }
        public VisualElement Footer { get; set; }
        public Label ManeuverNodeNumberLabel { get; set; }
        public Button PreviousNodeButton { get; set; }
        public Button NextNodeButton { get; set; }

        public EntryWindowController()
        { }

        public EntryWindowController(EntryWindow w, VisualElement windowRoot)
        {
            EntryWindow = w;
            WindowRoot = windowRoot;
            Initialize();
        }

        public void Initialize()
        {
            Root = Uxmls.Instance.EntryWindow.CloneTree();

            BuildTitle();
            BuildHeader();
            BuildBody();
            BuildFooter();

            if (EntryWindow.IsFlightActive)
                Expand();
            else
                Collapse();

            WindowRoot[0].RegisterCallback<PointerUpEvent>(UpdateWindowPosition);
            WindowRoot[0].transform.position = EntryWindow.FlightRect.position;

            // Hide the settings button if window is not editable (Stage window)
            if (!EntryWindow.IsEditable)
                SettingsButton.style.display = DisplayStyle.None;

            if (EntryWindow is ManeuverWindow)
            {
                CheckIfManeuverHeaderAndFooterShouldBeHidden();
                UpdateManeuverNodeHeader();
            }
        }

        public void UpdateWindowPosition(PointerUpEvent evt)
        {
            if (EntryWindow == null || !EntryWindow.IsFlightPoppedOut)
                return;

            EntryWindow.FlightRect.position = WindowRoot[0].transform.position;
            Utility.SaveLayout();
        }

        private void BuildTitle()
        {
            Title = Root.Q<VisualElement>("title");
            Title.AddManipulator(new Clickable(OnTitleClick));
            TitleArrowDown = Root.Q<VisualElement>("title-arrow-down");
            TitleArrowRight = Root.Q<VisualElement>("title-arrow-right");
            NameLabel = Root.Q<Label>("window-name");
            NameLabel.text = EntryWindow.Name;
            SettingsButton = Root.Q<Button>("settings-button");
            SettingsButton.RegisterCallback<ClickEvent>(OpenSettingsWindow);
            PopOutButton = Root.Q<Button>("popout-button");
            PopOutButton.RegisterCallback<ClickEvent>(OnPopOutOrCloseButton);
            CloseButton = Root.Q<Button>("close-button");
            CloseButton.RegisterCallback<ClickEvent>(OnPopOutOrCloseButton);

            if (EntryWindow.IsFlightPoppedOut)
            {
                PopOutButton.style.display = DisplayStyle.None;

                if (EntryWindow.IsLocked)
                    CloseButton.style.display = DisplayStyle.None;
            }
            else
                CloseButton.style.display = DisplayStyle.None;
        }

        private void OpenSettingsWindow(ClickEvent evt)
        {
            var editableWindowId = FlightSceneController.Instance.GetEditableWindows().FindIndex(w => w.Name == EntryWindow.Name);
            FlightSceneController.Instance.ToggleEditWindows(true, editableWindowId);
        }

        private void BuildHeader()
        {
            Header = Root.Q<VisualElement>("header");

            if (EntryWindow is StageWindow)
            {
                Header.Add(Uxmls.Instance.StageInfoHeader.CloneTree());
                return;
            }

            if (EntryWindow is ManeuverWindow)
                BuildManeuverHeader(EntryWindow as ManeuverWindow);
        }        

        private void BuildBody()
        {
            Body = Root.Q<VisualElement>("body");

            foreach (var entry in EntryWindow.Entries)
            {
                VisualElement control;

                switch (entry.EntryType)
                {
                    case EntryType.BasicText:
                        control = new BaseEntryControl();
                        InitializeControl((BaseEntryControl)control, entry, true);
                        break;
                    case EntryType.Time:
                        control = new TimeEntryControl();
                        InitializeTimeEntryControl((TimeEntryControl)control, entry);
                        break;
                    case EntryType.LatitudeLongitude:
                        control = new LatLonEntryControl();
                        InitializeLatLonEntryControl((LatLonEntryControl)control, entry);
                        break;
                    case EntryType.StageInfo:
                        control = new StageInfoEntriesBuilder(entry);
                        break;
                    case EntryType.Separator:
                        control = new SeparatorEntryControl();
                        break;
                    default:
                        control = new VisualElement();
                        break;
                }

                Body.Add(control);
            }
        }

        private void BuildFooter()
        {
            Footer = Root.Q<VisualElement>("footer");

            if (EntryWindow is ManeuverWindow)
                BuildManeuverFooter(EntryWindow as ManeuverWindow);
        }        

        public void Expand()
        {
            Title.AddToClassList("window-title__active");
            TitleArrowDown.style.display = DisplayStyle.Flex;
            TitleArrowRight.style.display = DisplayStyle.None;
            Header.style.display = DisplayStyle.Flex;
            Body.style.display = DisplayStyle.Flex;
            Footer.style.display = DisplayStyle.Flex;

            if (EntryWindow is ManeuverWindow)
            {
                CheckIfManeuverHeaderAndFooterShouldBeHidden();
                UpdateManeuverNodeHeader();
            }
        }

        public void Collapse()
        {
            Title.RemoveFromClassList("window-title__active");
            TitleArrowDown.style.display = DisplayStyle.None;
            TitleArrowRight.style.display = DisplayStyle.Flex;
            Header.style.display = DisplayStyle.None;
            Body.style.display = DisplayStyle.None;
            Footer.style.display = DisplayStyle.None;
        }

        private void OnTitleClick(EventBase evt)
        {
            EntryWindow.IsFlightActive = !EntryWindow.IsFlightActive;

            if (EntryWindow.IsFlightActive)
                Expand();
            else
                Collapse();
        }

        private void OnPopOutOrCloseButton(ClickEvent evt)
        {
            EntryWindow.IsFlightPoppedOut = !EntryWindow.IsFlightPoppedOut;
            
            // Activate/expand windows that get popped out.
            if (EntryWindow.IsFlightPoppedOut)
                EntryWindow.IsFlightActive = true;

            Utility.SaveLayout();
            FlightSceneController.Instance.RebuildUI();
        }

        /// <summary>
        /// Not used
        /// </summary>
        public void SetWindowVisibility(bool isVisible)
        {
            if (isVisible)
                Root.style.display = DisplayStyle.Flex;
            else
                Root.style.display = DisplayStyle.None;
        }
        
        ///// Entry control initialization /////
        
        public void InitializeControl(BaseEntryControl control, BaseEntry entry, bool subscribeToValueChanges = true)
        {
            control.EntryName = entry.Name;
            control.Value = entry.ValueDisplay;
            control.Unit = entry.UnitDisplay;

            // When entry's value changes, update value and unit labels
            if (subscribeToValueChanges)
                entry.OnEntryValueChanged += (value, unit, hideWhenNoData) => HandleEntryValueChanged(control, value, unit, hideWhenNoData);

            // Handle alternate units
            if (entry.AltUnit != null)
                control.RegisterCallback<MouseDownEvent>(_ => ToggleAltUnit(control, entry), TrickleDown.TrickleDown);
            
            HandleEntryValueChanged(control, control.Value, control.Unit, entry.HideWhenNoData);
        }
        
        // BaseEntryControl initialization
        public void HandleEntryValueChanged(BaseEntryControl control, string value, string unit, bool hideWhenNoData)
        {
            if (hideWhenNoData)
            {
                if (value == "-" || string.IsNullOrEmpty(value))
                    control.style.display = DisplayStyle.None;
                else
                    control.style.display = DisplayStyle.Flex;
            }
            
            control.Value = value;
            if (control.Unit != unit)
                control.Unit = unit;
        }
        
        public void ToggleAltUnit(BaseEntryControl control, BaseEntry entry)
        {
            if (Time.time - control.TimeOfLastClick < 0.5f)
                entry.AltUnit.IsActive = !entry.AltUnit.IsActive;

            control.TimeOfLastClick = Time.time;
        }
        
        // TimeEntryControl initialization
        public void HandleEntryTimeValueChanged(TimeEntryControl control, int years, int days, int hours, int minutes, int seconds) => control.SetValue(years, days, hours, minutes, seconds);
        
        public void InitializeTimeEntryControl(TimeEntryControl control, BaseEntry entry)
        {
            control.EntryName = entry.Name;

            var time = Utility.ParseSecondsToTimeFormat((double?)entry.EntryValue ?? 0);
            control.SetValue(time.Years, time.Days, time.Hours, time.Minutes, time.Seconds);

            entry.OnEntryTimeValueChanged += (years, days, hours, minutes, seconds) => HandleEntryTimeValueChanged(control, years, days, hours, minutes, seconds);
        }
        
        //LatLonEntryControl initialization
        
        public void HandleEntryLatLonChanged(LatLonEntryControl control, int degrees, int minutes, int seconds, string direction) => control.SetValue(degrees, minutes, seconds, direction);
        
        public void InitializeLatLonEntryControl(LatLonEntryControl control, BaseEntry entry)
        {
            control.EntryName = entry.Name;

            var latLon = Utility.ParseDegreesToDMSFormat((double?)entry.EntryValue ?? 0);
            control.SetValue(latLon.Degrees, latLon.Minutes, latLon.Seconds, entry.BaseUnit);

            entry.OnEntryLatLonChanged += (degrees, minutes, seconds, direction) => HandleEntryLatLonChanged(control, degrees, minutes, seconds, direction);
        }
        

        ///// MANEUVER UI /////

        private void BuildManeuverHeader(ManeuverWindow window)
        {
            var maneuverHeader = Uxmls.Instance.ManeuverHeader.CloneTree();
            PreviousNodeButton = maneuverHeader.Q<Button>("previous-node");
            ManeuverNodeNumberLabel = maneuverHeader.Q<Label>("node-number");

            PreviousNodeButton.RegisterCallback<MouseUpEvent>(_ =>
            {
                int index = window.SelectPreviousNode();
            });

            NextNodeButton = maneuverHeader.Q<Button>("next-node");
            NextNodeButton.RegisterCallback<MouseUpEvent>(_ =>
            {
                int index = window.SelectNextNode();
            });

            Header.Add(maneuverHeader);
        }

        private void BuildManeuverFooter(ManeuverWindow window)
        {
            var maneuverFooter = Uxmls.Instance.ManeuverFooter.CloneTree();
            var deleteNodeButton = maneuverFooter.Q<Button>("delete-node");
            deleteNodeButton.RegisterCallback<MouseUpEvent>(_ =>
            {
                int index = window.DeleteNodes();
                ManeuverNodeNumberLabel.text = $"Node #{index + 1}";
            });

            Footer.Add(maneuverFooter);
            
            window.OnNodeCountChanged += CheckIfManeuverHeaderAndFooterShouldBeHidden;
            window.OnSelectedNodeIndexChanged += UpdateManeuverNodeHeader;
        }

        private void CheckIfManeuverHeaderAndFooterShouldBeHidden()
        {
            int nodeCount = ((ManeuverWindow)EntryWindow).NodeCount;

            Header.style.display = nodeCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            Footer.style.display = nodeCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateManeuverNodeHeader()
        {
            int selectedNodeIndex = ((ManeuverWindow)EntryWindow).SelectedNodeIndex;
            int nodeCount = ((ManeuverWindow)EntryWindow).NodeCount;

            ManeuverNodeNumberLabel.text = $"Node #{(selectedNodeIndex + 1)}";
            if (selectedNodeIndex == 0)
                PreviousNodeButton.SetEnabled(false);
            else
                PreviousNodeButton.SetEnabled(true);

            if (selectedNodeIndex >= nodeCount - 1)
                NextNodeButton.SetEnabled(false);
            else
                NextNodeButton.SetEnabled(true);
        }
    }
}
