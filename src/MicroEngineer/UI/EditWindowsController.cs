using MicroEngineer.Entries;
using MicroEngineer.Managers;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UnityEngine;
using UnityEngine.UIElements;
using UitkForKsp2.API;

namespace MicroEngineer.UI
{
    public class EditWindowsController : MonoBehaviour
    {
        private (EditWindowsItemControl control, BaseEntry entry) _selectedAvailableEntry;
        private (EditWindowsItemControl control, BaseEntry entry) _selectedInstalledEntry;
        private List<EntryWindow> _editableWindows;
        private List<(EditWindowsItemControl control, BaseEntry entry)> _installedControls = new();
        private float _timeOfLastClick;
        
        public int SelectedWindowId;

        public UIDocument EditWindows { get; set; }
        public VisualElement Root { get; set; }
        public Button CloseButton { get; set; }
        public ScrollView AvailableScrollView { get; set; }
        public ScrollView InstalledScrollView { get; set; }
        public DropdownField CategoryDropdown { get; set; }
        public TextField SelectedWindow { get; set; }
        public Button PreviousWindow { get; set; }
        public Button NextWindow { get; set; }
        public Button NewWindow { get; set; }
        public Button DeleteWindow { get; set; }
        public Toggle LockWindow { get; set; }
        public Button AddEntry { get; set; }
        public Button RemoveEntry { get; set; }
        public Button MoveUp { get; set; }
        public Button MoveDown { get; set; }

        public EditWindowsController()
        { }

        private void OnEnable()
        {
            StartCoroutine(StartInitialization());
        }

        private System.Collections.IEnumerator StartInitialization()
        {
            // wait for 1 frame until SelectedWindowId is set in FlightSceneController
            yield return null;

            EditWindows = GetComponent<UIDocument>();
            Root = EditWindows.rootVisualElement;

            CloseButton = Root.Q<Button>("close-button");
            CloseButton.RegisterCallback<PointerUpEvent>(_ => CloseWindow());
            AvailableScrollView = Root.Q<ScrollView>("available-scrollview");
            InstalledScrollView = Root.Q<ScrollView>("installed-scrollview");
            CategoryDropdown = Root.Q<DropdownField>("category__dropdown");
            SelectedWindow = Root.Q<TextField>("selected-window");
            SelectedWindow.RegisterValueChangedCallback(RenameWindow);
            PreviousWindow = Root.Q<Button>("prev-window");
            PreviousWindow.RegisterCallback<PointerUpEvent>(SelectPreviousWindow);
            NextWindow = Root.Q<Button>("next-window");
            NextWindow.RegisterCallback<PointerUpEvent>(SelectNextWindow);
            NewWindow = Root.Q<Button>("new-window");
            NewWindow.RegisterCallback<PointerUpEvent>(CreateNewWindow);
            DeleteWindow = Root.Q<Button>("delete-window");
            DeleteWindow.RegisterCallback<PointerUpEvent>(DeleteDeletableWindow);
            LockWindow = Root.Q<Toggle>("lock-window");
            LockWindow.RegisterValueChangedCallback(LockUnlockWindow);
            AddEntry = Root.Q<Button>("add-entry");
            AddEntry.RegisterCallback<PointerUpEvent>(evt => AddEntryToSelectedWindow(_selectedAvailableEntry.entry));
            AddEntry.SetEnabled(false);
            RemoveEntry = Root.Q<Button>("remove-entry");
            RemoveEntry.RegisterCallback<PointerUpEvent>(evt => RemoveEntryFromInstalledWindow());
            RemoveEntry.SetEnabled(false);
            MoveUp = Root.Q<Button>("move-up");
            MoveUp.RegisterCallback<PointerUpEvent>(MoveEntryUp);
            MoveDown = Root.Q<Button>("move-down");
            MoveDown.RegisterCallback<PointerUpEvent>(MoveEntryDown);

            BuildCategoryDropdown();
            _editableWindows = FlightSceneController.Instance.GetEditableWindows();
            ResetSelectedWindow();
        }

        public void Update()
        {
            return;
        }

        //////////////////////   AVAILABLE (LEFT SCROLLVIEW) //////////////////////
        private void BuildCategoryDropdown()
        {
            CategoryDropdown.choices = Enum.GetNames(typeof(MicroEntryCategory)).Where(e => e != MicroEntryCategory.OAB.ToString()).ToList();
            CategoryDropdown.RegisterValueChangedCallback(BuildAvailableEntries);
        }

        private void BuildAvailableEntries(ChangeEvent<string> _)
        {
            AvailableScrollView.Clear();
            _selectedAvailableEntry = (null, null);
            AddEntry.SetEnabled(false);
            if (CategoryDropdown.value == null)
                return;

            List<BaseEntry> entriesByCategory = Manager.Instance.Entries.FindAll(e => e.Category.ToString() == CategoryDropdown.value); // All entries belong to a category, but they can still be placed in any window
            foreach (var e in entriesByCategory)
            {
                var control = new EditWindowsItemControl(e.Name, true);
                var textField = control.Q<TextField>();
                textField.RegisterCallback<MouseDownEvent>(evt => OnAvailableEntryClicked(evt, control, e));
                AvailableScrollView.Add(control);
            }
        }

        private void OnAvailableEntryClicked(MouseDownEvent evt, EditWindowsItemControl control, BaseEntry entry)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (control != _selectedAvailableEntry.control)
                    SelectAvailable(control, entry);
                else if (Time.time - _timeOfLastClick < 0.5f)
                    AddEntryToSelectedWindow(entry); // double click on control
                else
                    UnselectAvailable(control); // longer than 500 ms since the last click, so we'll unselect the control
            }

            _timeOfLastClick = Time.time;
        }

        /// <summary>
        /// Implement your logic here when the label is selected
        /// For example, change the label's appearance or perform some action.
        /// </summary>
        public void SelectAvailable(EditWindowsItemControl control, BaseEntry entry)
        {
            if (_selectedAvailableEntry.control != null && _selectedAvailableEntry.control != control)
            {
                _selectedAvailableEntry.control.Unselect();
            }

            _selectedAvailableEntry = (control, entry);
            control.Select();
            AddEntry.SetEnabled(true);
        }

        /// <summary>
        /// Implement your logic here when the label is unselected
        /// For example, revert the label's appearance or perform some action.
        /// </summary>
        public void UnselectAvailable(EditWindowsItemControl control)
        {
            if (control == _selectedAvailableEntry.control)
            {
                _selectedAvailableEntry = (null, null);
            }
            control.Unselect();
            AddEntry.SetEnabled(false);
        }

        //////////////////////   INSTALLED (RIGHT SCROLLVIEW) //////////////////////

        public void ResetSelectedWindow()
        {
            SelectedWindow.SetValueWithoutNotify(_editableWindows[SelectedWindowId].Name);
            BuildInstalledEntries();
            RemoveEntry.SetEnabled(false);
            DeleteWindow.SetEnabled(_editableWindows[SelectedWindowId].IsDeletable);
            LockWindow.value = _editableWindows[SelectedWindowId].IsLocked;
        }

        private void BuildInstalledEntries()
        {
            _installedControls.Clear();
            InstalledScrollView.Clear();
            _selectedInstalledEntry = (null, null);

            List<BaseEntry> installedEntries = _editableWindows[SelectedWindowId].Entries;
            foreach (var e in installedEntries)
            {
                var control = new EditWindowsItemControl(e.Name, false);
                var textField = control.Q<TextField>();
                var incDecimal = control.Q<Button>("increase-decimal");
                var decDecimal = control.Q<Button>("decrease-decimal");
                incDecimal.RegisterCallback<PointerUpEvent>(_ => IncreaseDecimalDigits(e, incDecimal, decDecimal));
                decDecimal.RegisterCallback<PointerUpEvent>(_ => DecreaseDecimalDigits(e, incDecimal, decDecimal));
                CheckIfDecimalButtonsShouldBeEnabled(e, incDecimal, decDecimal);
                textField.RegisterCallback<MouseDownEvent>(evt => OnInstalledEntryClicked(evt, control, e));
                textField.RegisterValueChangedCallback(evt => RenameEntry(evt, e));
                textField.DisableGameInputOnFocus();
                _installedControls.Add((control, e));
                InstalledScrollView.Add(control);
            }
        }

        private void OnInstalledEntryClicked(MouseDownEvent evt, EditWindowsItemControl control, BaseEntry entry)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (control != _selectedInstalledEntry.control)
                    SelectInstalled(control, entry);
                else if (Time.time - _timeOfLastClick < 0.5f)
                    RemoveEntryFromInstalledWindow(); // double click on control - doesn't work with UitkForKsp2 v2.1.1
                else
                    UnselectInstalled(control); // longer than 500 ms since the last click, so we'll unselect the control
            }

            _timeOfLastClick = Time.time;
        }

        public void SelectInstalled(EditWindowsItemControl control, BaseEntry entry)
        {
            if (_selectedInstalledEntry.control != null && _selectedInstalledEntry.control != control)
            {
                _selectedInstalledEntry.control.Unselect();
            }

            _selectedInstalledEntry = (control, entry);
            control.Select();
            RemoveEntry.SetEnabled(true);
        }

        public void UnselectInstalled(EditWindowsItemControl control)
        {
            if (control == _selectedInstalledEntry.control)
            {
                _selectedInstalledEntry = (null, null);
            }
            control.Unselect();
            RemoveEntry.SetEnabled(false);
        }

        private void SelectPreviousWindow(PointerUpEvent evt)
        {
            if (SelectedWindowId > 0)
                SelectedWindowId--;
            else
                SelectedWindowId = _editableWindows.Count - 1;

            ResetSelectedWindow();
        }

        private void SelectNextWindow(PointerUpEvent evt)
        {
            if (SelectedWindowId < _editableWindows.Count-1)
                SelectedWindowId++;
            else
                SelectedWindowId = 0;

            ResetSelectedWindow();
        }

        private void MoveEntryUp(PointerUpEvent evt)
        {
            if (_selectedInstalledEntry.control == null)
                return;

            var index = _installedControls.IndexOf(_selectedInstalledEntry);
            if (index == 0)
                return;

            _editableWindows[SelectedWindowId].MoveEntryUp(_selectedInstalledEntry.entry);
            
            ResetSelectedWindow();
            RebuildFlightUI();

            var installed = _installedControls[index - 1];
            SelectInstalled(installed.control, installed.entry);
        }

        private void MoveEntryDown(PointerUpEvent evt)
        {
            if (_selectedInstalledEntry.control == null)
                return;

            var index = _installedControls.IndexOf(_selectedInstalledEntry);
            if (index == _installedControls.Count - 1)
                return;

            _editableWindows[SelectedWindowId].MoveEntryDown(_selectedInstalledEntry.entry);

            ResetSelectedWindow();
            RebuildFlightUI();

            var installed = _installedControls[index + 1];
            SelectInstalled(installed.control, installed.entry);
        }

        private void AddEntryToSelectedWindow(BaseEntry entry)
        {
            _editableWindows[SelectedWindowId].AddEntry(Activator.CreateInstance(entry.GetType()) as BaseEntry);

            UnselectAvailable(_selectedAvailableEntry.control);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void RemoveEntryFromInstalledWindow()
        {
            _editableWindows[SelectedWindowId].RemoveEntry(_selectedInstalledEntry.entry);
            UnselectInstalled(_selectedInstalledEntry.control);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void RenameEntry(ChangeEvent<string> evt, BaseEntry entry)
        {
            entry.Name = evt.newValue;
            RebuildFlightUI();
        }

        private void RenameWindow(ChangeEvent<string> evt)
        {
            _editableWindows[SelectedWindowId].Name = evt.newValue;
            RebuildFlightUI();
        }

        private void CreateNewWindow(PointerUpEvent evt)
        {
            SelectedWindowId = Manager.Instance.CreateCustomWindow(_editableWindows);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void DeleteDeletableWindow(PointerUpEvent evt)
        {
            if (_editableWindows[SelectedWindowId].IsDeletable)
            {
                Manager.Instance.Windows.Remove(_editableWindows[SelectedWindowId]);
                _editableWindows.Remove(_editableWindows[SelectedWindowId]);
                SelectedWindowId--;
            }
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void LockUnlockWindow(ChangeEvent<bool> evt)
        {
            _editableWindows[SelectedWindowId].IsLocked = evt.newValue;
            if (evt.newValue)
                _editableWindows[SelectedWindowId].IsFlightActive = true;

            RebuildFlightUI();
        }

        private void IncreaseDecimalDigits(BaseEntry entry, Button incDecimal, Button decDecimal)
        {
            entry.NumberOfDecimalDigits++;
            CheckIfDecimalButtonsShouldBeEnabled(entry, incDecimal, decDecimal);
            RebuildFlightUI();
        }

        private void DecreaseDecimalDigits(BaseEntry entry, Button incDecimal, Button decDecimal)
        {
            entry.NumberOfDecimalDigits--;
            CheckIfDecimalButtonsShouldBeEnabled(entry, incDecimal, decDecimal);
            RebuildFlightUI();
        }

        private void CheckIfDecimalButtonsShouldBeEnabled(BaseEntry entry, Button increase, Button decrease)
        {
            if (entry.Formatting == null)
            {
                increase.SetEnabled(false);
                decrease.SetEnabled(false);
            }
            else
            {
                increase.SetEnabled(entry.NumberOfDecimalDigits < 5);
                decrease.SetEnabled(entry.NumberOfDecimalDigits > 0);
            }
        }

        private void RebuildFlightUI()
        {
            Utility.SaveLayout();
            FlightSceneController.Instance.RebuildUI();
        }

        public void CloseWindow()
        {
            if (EditWindows != null && EditWindows.gameObject != null)
                EditWindows.gameObject.DestroyGameObject();
            GameObject.Destroy(EditWindows);
        }
    }
}
