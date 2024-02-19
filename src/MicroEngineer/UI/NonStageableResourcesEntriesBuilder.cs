using MicroEngineer.Entries;
using MicroEngineer.Utilities;
using UnityEngine.UIElements;

namespace MicroEngineer.UI;

public class NonStageableResourcesEntriesBuilder : VisualElement
{
    private DateTime _timeOfLastClick = DateTime.Now;
    private BaseEntry _entry;
    
    public NonStageableResourcesEntriesBuilder(BaseEntry entry)
    {
        BuildNonStageableEntries((List<NonStageableResource>)entry.EntryValue, entry);
        
        entry.OnNonStageableResourcesChanged += (resources) => HandleStageInfoChanged(resources, entry);
        
        _entry = entry;
    }

    private void BuildNonStageableEntries(List<NonStageableResource> resources, BaseEntry entry)
    {
        foreach (NonStageableResource resource in resources ?? Enumerable.Empty<NonStageableResource>())
        {
            var unitFormatted = String.Format(entry.Formatting, resource.StoredUnits);
            if (!entry.AltUnit.IsActive)
            {
                unitFormatted += "/" + String.Format(entry.Formatting, resource.CapacityUnits);
            }
            
            this.Add(new BaseEntryControl(
                resource.Name,
                unitFormatted,
                resource.Unit));
        }
        
        // Handle alternate units
        if (entry.AltUnit != null)
            this.RegisterCallback<MouseDownEvent>(_ => ToggleAltUnit(), TrickleDown.TrickleDown);
    }

    private void HandleStageInfoChanged(List<NonStageableResource> resources, BaseEntry entry)
    {
        this.Clear();
        BuildNonStageableEntries(resources, entry);
    }

    private void ToggleAltUnit()
    {
        if ((DateTime.Now - _timeOfLastClick).TotalSeconds < 0.5f)
            _entry.AltUnit.IsActive = !_entry.AltUnit.IsActive;

        _timeOfLastClick = DateTime.Now;
    }
}