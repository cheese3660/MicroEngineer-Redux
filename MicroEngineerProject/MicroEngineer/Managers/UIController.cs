﻿using System.ComponentModel;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.UIElements;

namespace MicroMod
{
    public class UitkControllerTest
    {
        private static UitkControllerTest _instance;
        private UIDocument _window;

        public bool IsInitialized;
        public bool ShowWindow;

        public Label EntryValue;

        private UitkControllerTest()
        { }

        public static UitkControllerTest Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UitkControllerTest();

                return _instance;
            }
        }

        public void Initialize()
        {
            _window = Window.CreateFromUxml(Styles.uxml, null, null, true);
            IsInitialized = true;
            ShowWindow = true;

            //var x = Styles.uxml.visualElementAssets.FirstOrDefault();
            var root = _window.rootVisualElement;

            root.transform.position = new Vector3(1000, 200, 0);

            //EntryValue = root.Q<Name>("sample-_name");

            EntryWindow surfaceWindow = Manager.Instance.Windows.Find(w => w is EntryWindow && ((EntryWindow)w).Name == "Surface") as EntryWindow;
            //((AltitudeAgl)surfaceWindow.Entries[0])._name = EntryValue;

            var body = root.Q<VisualElement>("body");

            /* SUCCESSFUL TEST
            var entry = ((AltitudeAgl)surfaceWindow.Entries[0]);
            var entryControl = new BaseEntryControl();
            entryControl.Name = entry.Name;
            entry.OnEntryValueChanged += entryControl.HandleValueDisplayChanged;
            entryControl.Unit = entry.UnitDisplay;
            body.Add(entryControl);
            */

            
            while (body.childCount > 0)
                body.RemoveAt(0);
            
            
            /*
            foreach (var child in body.Children())
            {
                child.RemoveFromHierarchy();
            }
            */
            

            foreach (var entry in surfaceWindow.Entries)
            {
                var control = new BaseEntryControl(entry.Name, entry.ValueDisplay, entry.UnitDisplay);
                entry.OnEntryValueChanged += control.HandleValueDisplayChanged;
                body.Add(control);
            }

            
            
            



            /*
            var entryName = root.Q<Name>("entry-name");
            EntryValue = root.Q<Name>("entry-_value");
            var entryUnit = root.Q<Name>("entry-_unit");

            entryName.text = entry.Name;
            EntryValue.text = entry.ValueDisplay;
            entryUnit.text = entry.UnitDisplay;
            */
            


            //EntryValue.text = surfaceWindow.Entries[0].ValueDisplay;

            var firstVisualElement = root[0];
            /*
            var newLabel = new Name() { name = "new _name", text = "text for new _name" };
            newLabel.visible = true;
            newLabel.enabledSelf = true;
            firstVisualElement.Add(newLabel);
            */
        }

        // Listener method to handle the updated _value
        private void HandleValueDisplayChanged(string value)
        {
            // Use the updated _value to bind it to the control or perform other actions
            EntryValue.text = value;
            //Manager._logger.LogInfo("BaseEntry HandleValueDisplayChanged triggered");
        }



        public void Toggle()
        {
            if (ShowWindow)
            {
                _window.rootVisualElement.style.display = DisplayStyle.None;
            }
            else
            {
                _window.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            ShowWindow = !ShowWindow;

            //_window.enabled = !_window.enabled;
        }
    }

    public class Test
    {
        public void HandleValueDisplayChanged(string value)
        {
            // Use the updated _value to bind it to the control or perform other actions
            //EntryValue.text = _value;
            //Manager._logger.LogInfo("HandleValueDisplayChanged triggered");
        }
    }
}
