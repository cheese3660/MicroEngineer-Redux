using Newtonsoft.Json;

namespace MicroEngineer.Utilities;

/// <summary>
/// An alternative unit, activated by double-clicking the entry
/// </summary>
public class AltUnit
{
    [JsonProperty]
    public bool IsActive;
    [JsonProperty]
    public string Unit;
    [JsonProperty]
    public float Factor;
}
