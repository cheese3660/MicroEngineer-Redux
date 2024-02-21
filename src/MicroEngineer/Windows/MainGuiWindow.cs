using Newtonsoft.Json;

namespace MicroEngineer.Windows;

public class MainGuiWindow : BaseWindow
{
    [JsonProperty]
    public int LayoutVersion;
    [JsonProperty]
    public bool IsFlightMinimized;
}