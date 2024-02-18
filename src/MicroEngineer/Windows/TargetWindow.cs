using MicroEngineer.UI;
using MicroEngineer.Utilities;

namespace MicroEngineer.Windows;
    
public class TargetWindow : EntryWindow
{
    public override void RefreshData()
    {
        base.RefreshData();

        // Toggle showing/hiding UI window depending on whether a target exists
        FlightSceneController.Instance.TargetWindowShown = Utility.TargetExists();
    }
}
