﻿using SpaceWarp.API.UI;
using UnityEngine;

namespace MicroMod
{
    public static class MicroStyles
    {
        public static int WindowWidth = 290;
        public static int WindowHeight = 1440;

        public static GUISkin SpaceWarpUISkin;
        public static GUIStyle MainWindowStyle;
        public static GUIStyle PopoutWindowStyle;
        public static GUIStyle EditWindowStyle;
        public static GUIStyle PopoutBtnStyle;
        public static GUIStyle SectionToggleStyle;
        public static GUIStyle NameLabelStyle;
        public static GUIStyle ValueLabelStyle;
        public static GUIStyle BlueLabelStyle;
        public static GUIStyle UnitLabelStyle;
        public static GUIStyle NormalLabelStyle;
        public static GUIStyle TitleLabelStyle;
        public static GUIStyle NormalCenteredLabelStyle;
        public static GUIStyle WindowSelectionTextFieldStyle;
        public static GUIStyle WindowSelectionAbbrevitionTextFieldStyle;
        public static GUIStyle CloseBtnStyle;
        public static GUIStyle NormalBtnStyle;
        public static GUIStyle OneCharacterBtnStyle;
        public static GUIStyle TableHeaderLabelStyle;

        public static string UnitColorHex { get => ColorUtility.ToHtmlStringRGBA(UnitLabelStyle.normal.textColor); }

        public static int SpacingAfterHeader = -12;
        public static int SpacingAfterEntry = -12;
        public static int SpacingAfterSection = 5;
        public static float SpacingBelowPopout = 10;

        public static float PoppedOutX = Screen.width * 0.6f;
        public static float PoppedOutY = Screen.height * 0.2f;
        public static float MainGuiX = Screen.width * 0.8f;
        public static float MainGuiY = Screen.height * 0.2f;

        public static Rect CloseBtnRect = new Rect(MicroStyles.WindowWidth - 23, 6, 16, 16);
        public static Rect EditWindowRect = new Rect(Screen.width * 0.5f - MicroStyles.WindowWidth / 2, Screen.height * 0.2f, MicroStyles.WindowWidth, 0);

        public static void InitializeStyles()
        {
            SpaceWarpUISkin = Skins.ConsoleSkin;

            MainWindowStyle = new GUIStyle(SpaceWarpUISkin.window)
            {
                padding = new RectOffset(8, 8, 20, 8),
                contentOffset = new Vector2(0, -22),
                fixedWidth = WindowWidth
            };

            PopoutWindowStyle = new GUIStyle(MainWindowStyle)
            {
                padding = new RectOffset(MainWindowStyle.padding.left, MainWindowStyle.padding.right, 0, MainWindowStyle.padding.bottom - 5),
                fixedWidth = WindowWidth
            };

            EditWindowStyle = new GUIStyle(PopoutWindowStyle)
            {
                padding = new RectOffset(8, 8, 30, 8)
            };

            PopoutBtnStyle = new GUIStyle(SpaceWarpUISkin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                contentOffset = new Vector2(0, 2),
                fixedHeight = 15,
                fixedWidth = 15,
                fontSize = 28,
                clipping = TextClipping.Overflow,
                margin = new RectOffset(0, 0, 10, 0)
            };

            SectionToggleStyle = new GUIStyle(SpaceWarpUISkin.toggle)
            {
                padding = new RectOffset(14, 0, 3, 3)
            };

            NameLabelStyle = new GUIStyle(SpaceWarpUISkin.label);
            NameLabelStyle.normal.textColor = new Color(.7f, .75f, .75f, 1);

            ValueLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                alignment = TextAnchor.MiddleRight
            };
            ValueLabelStyle.normal.textColor = new Color(.6f, .7f, 1, 1);

            UnitLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                fixedWidth = 24,
                alignment = TextAnchor.MiddleLeft
            };
            UnitLabelStyle.normal.textColor = new Color(.7f, .75f, .75f, 1);

            NormalLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                fixedWidth = 120
            };

            TitleLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                fontSize = 18,
                fixedWidth = 100,
                fixedHeight = 50,
                contentOffset = new Vector2(0, -20),
            };

            NormalCenteredLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                fixedWidth = 80,
                alignment = TextAnchor.MiddleCenter
            };

            BlueLabelStyle = new GUIStyle(SpaceWarpUISkin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            BlueLabelStyle.normal.textColor = new Color(.6f, .7f, 1, 1);

            WindowSelectionTextFieldStyle = new GUIStyle(SpaceWarpUISkin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 80
            };

            WindowSelectionAbbrevitionTextFieldStyle = new GUIStyle(SpaceWarpUISkin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 40
            };

            CloseBtnStyle = new GUIStyle(SpaceWarpUISkin.button)
            {
                fontSize = 8
            };

            NormalBtnStyle = new GUIStyle(SpaceWarpUISkin.button)
            {
                alignment = TextAnchor.MiddleCenter
            };

            OneCharacterBtnStyle = new GUIStyle(SpaceWarpUISkin.button)
            {
                fixedWidth = 20,
                alignment = TextAnchor.MiddleCenter
            };

            TableHeaderLabelStyle = new GUIStyle(NameLabelStyle)
            {
                alignment = TextAnchor.MiddleRight
            };
        }

        /// <summary>
        /// Draws a white horizontal line accross the container it's put in
        /// </summary>
        /// <param name="height">Height/thickness of the line</param>
        public static void DrawHorizontalLine(float height)
        {
            Texture2D horizontalLineTexture = new Texture2D(1, 1);
            horizontalLineTexture.SetPixel(0, 0, Color.white);
            horizontalLineTexture.Apply();
            GUI.DrawTexture(GUILayoutUtility.GetRect(Screen.width, height), horizontalLineTexture);
        }

        /// <summary>
        /// Draws a white horizontal line accross the container it's put in with height of 1 px
        /// </summary>
        public static void DrawHorizontalLine() { MicroStyles.DrawHorizontalLine(1); }
    }
}
