﻿using KSP.Game;
using KSP.Messages;
using MicroEngineer.Entries;
using MicroEngineer.UI;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using ReduxLib.Logging;

namespace MicroEngineer.Managers;

public class MessageManager
{
    private static readonly ILogger _logger = ReduxLib.ReduxLib.GetLogger("MicroEngineer.MessageManager");
    private static MessageManager _instance;

    public MessageManager()
    { }

    public static MessageManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MessageManager();

            return _instance;
        }
    }

    /// <summary>
    /// Subscribe to KSP2 messages
    /// </summary>
    public void SubscribeToMessages()
    {
        Utility.RefreshGameManager();

        // While in OAB we use the VesselDeltaVCalculationMessage event to refresh data as it's triggered a lot less frequently than Update()
        Utility.MessageCenter.PersistentSubscribe<VesselDeltaVCalculationMessage>(new Action<MessageCenterMessage>(obj => this.RefreshStagingDataOAB((VesselDeltaVCalculationMessage)obj)));

        // We are loading layout state when entering Flight or OAB game state
        Utility.MessageCenter.PersistentSubscribe<GameStateEnteredMessage>(new Action<MessageCenterMessage>(this.GameStateEntered));

        // We are saving layout state when exiting from Flight or OAB game state
        Utility.MessageCenter.PersistentSubscribe<GameStateLeftMessage>(new Action<MessageCenterMessage>(this.GameStateLeft));

        // Sets the selected node index to the newly created node
        Utility.MessageCenter.PersistentSubscribe<ManeuverCreatedMessage>(new Action<MessageCenterMessage>(this.OnManeuverCreatedMessage));

        // Resets node index
        Utility.MessageCenter.PersistentSubscribe<ManeuverRemovedMessage>(new Action<MessageCenterMessage>(this.OnManeuverRemovedMessage));

        // Torque update for StageInfoOAB
        Utility.MessageCenter.PersistentSubscribe<PartManipulationCompletedMessage>(new Action<MessageCenterMessage>(this.OnPartManipulationCompletedMessage));
    }

    private void OnManeuverCreatedMessage(MessageCenterMessage message)
    {
        var maneuverWindow = Manager.Instance.Windows.Find(w => w.GetType() == typeof(ManeuverWindow)) as ManeuverWindow;
        maneuverWindow.OnManeuverCreatedMessage(message);
    }

    private void OnManeuverRemovedMessage(MessageCenterMessage message)
    {
        var maneuverWindow = Manager.Instance.Windows.Find(w => w.GetType() == typeof(ManeuverWindow)) as ManeuverWindow;
        maneuverWindow.OnManeuverRemovedMessage(message);
    }

    private void OnPartManipulationCompletedMessage(MessageCenterMessage obj)
    {
        var torque = ((StageInfoOabWindow)Manager.Instance.Windows.Find(w => w is StageInfoOabWindow)).Entries.Find(e => e is Torque);
        torque.RefreshData();
    }

    private void GameStateEntered(MessageCenterMessage obj)
    {
        Utility.RefreshGameManager();
        _logger.LogDebug($"Entered GameStateEntered. GameState: {Utility.GameState.GameState}." +
            $"MainGui.IsFlightActive: {Manager.Instance.Windows.OfType<MainGuiWindow>().FirstOrDefault().IsFlightActive}." +
            $"StageOab.IsEditorActive: {Manager.Instance.Windows.OfType<StageInfoOabWindow>().FirstOrDefault().IsEditorActive}.");

        if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.VehicleAssemblyBuilder || Utility.GameState.GameState == GameState.Map3DView)
        {
            Utility.LoadLayout(Manager.Instance.Windows);

            if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView)
            {
                FlightSceneController.Instance.ShowGui = Manager.Instance.Windows.OfType<MainGuiWindow>().FirstOrDefault().IsFlightActive;
            }

            if (Utility.GameState.GameState == GameState.VehicleAssemblyBuilder)
            {
                OABSceneController.Instance.ShowGui = Manager.Instance.Windows.OfType<StageInfoOabWindow>().FirstOrDefault().IsEditorActive;
            }
        }
    }

    private void GameStateLeft(MessageCenterMessage obj)
    {
        Utility.RefreshGameManager();
        var maingui = Manager.Instance.Windows.OfType<MainGuiWindow>().FirstOrDefault();
        var stageOab = Manager.Instance.Windows.OfType<StageInfoOabWindow>().FirstOrDefault();

        if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.VehicleAssemblyBuilder || Utility.GameState.GameState == GameState.Map3DView)
        {
            Utility.SaveLayout();

            if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView)
                FlightSceneController.Instance.ShowGui = false;

            if (Utility.GameState.GameState == GameState.VehicleAssemblyBuilder)
                OABSceneController.Instance.ShowGui = false;
        }
    }

    /// <summary>
    /// Refresh all staging data while in OAB
    /// </summary>
    public void RefreshStagingDataOAB(VesselDeltaVCalculationMessage msg = null)
    {
        // Check if message originated from ships in flight. If yes, return.
        if (msg != null && (msg.DeltaVComponent.Ship == null || !msg.DeltaVComponent.Ship.IsLaunchAssembly())) return;

        Utility.RefreshGameManager();
        if (Utility.GameState.GameState != GameState.VehicleAssemblyBuilder) return;

        Utility.RefreshStagesOAB();

        StageInfoOabWindow stageWindow = Manager.Instance.Windows.OfType<StageInfoOabWindow>().FirstOrDefault();

        if (Utility.VesselDeltaVComponentOAB?.StageInfo == null)
        {
            stageWindow.Entries.Find(e => e.Name == "Stage Info (OAB)").EntryValue = null;
            return;
        }

        stageWindow.RefreshData();
    }
}