using MicroEngineer.Entries;
using Newtonsoft.Json;

namespace MicroEngineer.Windows;

public class StageInfoOabWindow : BaseWindow
{
    [JsonProperty]
    public bool IsEditorPoppedOut;
    [JsonProperty]
    public List<BaseEntry> Entries;

    public virtual void RefreshData()
    {
        if (Entries == null || Entries.Count == 0)
            return;

        foreach (BaseEntry entry in Entries)
            entry.RefreshData();
    }
}
