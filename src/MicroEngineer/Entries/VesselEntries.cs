using KSP;
using KSP.Game;
using KSP.Sim.ResourceSystem;
using MicroEngineer.Utilities;

namespace MicroEngineer.Entries;

public class VesselEntry : BaseEntry
{ }

public class VesselName : VesselEntry
{
    public VesselName()
    {
        Name = "Name";
        Description = "Name of the current vessel.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        BaseUnit = null;
        Formatting = null;
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.DisplayName;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class Mass : VesselEntry
{
    public Mass()
    {
        Name = "Mass";
        Description = "Total mass of the vessel.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        MiliUnit = "g";
        BaseUnit = "kg";
        KiloUnit = "T";
        MegaUnit = "kT";
        GigaUnit = "MT";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.totalMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class DryMass : VesselEntry
{
    public DryMass()
    {
        Name = "Total Dry Mass";
        Description = "Total current dry mass.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "g";
        BaseUnit = "kg";
        KiloUnit = "T";
        MegaUnit = "kT";
        GigaUnit = "MT";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault()?.DryMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class FuelMass : VesselEntry
{
    public FuelMass()
    {
        Name = "Fuel Mass";
        Description = "Total fuel mass remaining on the vessel.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "g";
        BaseUnit = "kg";
        KiloUnit = "T";
        MegaUnit = "kT";
        GigaUnit = "MT";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault()?.FuelMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class TotalDeltaVActual : VesselEntry
{
    public TotalDeltaVActual()
    {
        Name = "Total ∆v";
        Description = "Vessel's total delta velocity.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        MiliUnit = "mm/s";
        BaseUnit = "m/s";
        KiloUnit = "km/s";
        MegaUnit = "Mm/s";
        GigaUnit = "Gm/s";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.TotalDeltaVActual;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class TotalDeltaVASL : VesselEntry
{
    public TotalDeltaVASL()
    {
        Name = "Total ∆v ASL";
        Description = "Total delta velocity of the vessel At Sea Level.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "mm/s";
        BaseUnit = "m/s";
        KiloUnit = "km/s";
        MegaUnit = "Mm/s";
        GigaUnit = "Gm/s";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.TotalDeltaVASL;
    }

    public override string ValueDisplay => base.ValueDisplay;

}

public class TotalDeltaVVac : VesselEntry
{
    public TotalDeltaVVac()
    {
        Name = "Total ∆v Vac";
        Description = "Total delta velocity of the vessel in vacuum.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "mm/s";
        BaseUnit = "m/s";
        KiloUnit = "km/s";
        MegaUnit = "Mm/s";
        GigaUnit = "Gm/s";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.TotalDeltaVVac;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class TotalBurnTime : VesselEntry
{
    public TotalBurnTime()
    {
        Name = "Total burn time";
        Description = "Burn Time vessel can sustain with 100% thrust.";
        EntryType = EntryType.Time;
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        Formatting = null;
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.TotalBurnTime;
    }
}

public class StageThrustActual : VesselEntry
{
    public StageThrustActual()
    {
        Name = "Thrust";
        Description = "Vessel's actual thrust.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        MiliUnit = "N";
        BaseUnit = "kN";
        KiloUnit = "MN";
        MegaUnit = "GN";
        GigaUnit = null;
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault()?.ThrustActual;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageThrustASL : VesselEntry
{
    public StageThrustASL()
    {
        Name = "Thrust (ASL)";
        Description = "Vessel's thrust At Sea Level.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "N";
        BaseUnit = "kN";
        KiloUnit = "MN";
        MegaUnit = "GN";
        GigaUnit = null;
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.ThrustASL * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageThrustVac : VesselEntry
{
    public StageThrustVac()
    {
        Name = "Thrust (vacuum)";
        Description = "Vessel's thrust in vacuum.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "N";
        BaseUnit = "kN";
        KiloUnit = "MN";
        MegaUnit = "GN";
        GigaUnit = null;
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.ThrustVac * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageTWRActual : VesselEntry
{
    public StageTWRActual()
    {
        Name = "TWR";
        Description = "Vessel's Thrust to Weight Ratio.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        BaseUnit = null;
        NumberOfDecimalDigits = 2;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault()?.TWRActual;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageTWRASL : VesselEntry
{
    public StageTWRASL()
    {
        Name = "TWR (ASL)";
        Description = "Vessel's Thrust to Weight Ratio At Sea Level.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = null;
        NumberOfDecimalDigits = 2;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.TWRASL;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageTWRVac : VesselEntry
{
    public StageTWRVac()
    {
        Name = "TWR (vacuum)";
        Description = "Vessel's Thrust to Weight Ratio in vacuum.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = null;
        NumberOfDecimalDigits = 2;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.TWRVac;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageISPActual : VesselEntry
{
    public StageISPActual()
    {
        Name = "ISP";
        Description = "Specific impulse (ISP) is a measure of how efficiently a reaction mass engine creates thrust.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "s";
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.IspActual;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageISPAsl : VesselEntry
{
    public StageISPAsl()
    {
        Name = "ISP (ASL)";
        Description = "Specific impulse At Sea Level.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "s";
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.IspASL;
    }

    public override string ValueDisplay => base.ValueDisplay;
}    

public class StageISPVac : VesselEntry
{
    public StageISPVac()
    {
        Name = "ISP (vacuum)";
        Description = "Specific impulse in vacuum.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "s";
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo?.FirstOrDefault()?.IspVac;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class FuelPercentage : VesselEntry
{
    public FuelPercentage()
    {
        Name = "Vessel fuel";
        Description = "Vessel's fuel percentage left.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "%";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.FuelPercentage;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class PartsCount : VesselEntry
{
    public PartsCount()
    {
        Name = "Parts";
        Description = "Number of parts vessel is constructed of.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = true;
        BaseUnit = null;
        Formatting = null;
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.PartInfo?.Count;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class Throttle : VesselEntry
{
    public Throttle()
    {
        Name = "Throttle";
        Description = "Vessel's current throttle in %.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "%";
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.flightCtrlState.mainThrottle * 100;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class TotalCommandCrewCapacity : VesselEntry
{
    public TotalCommandCrewCapacity()
    {
        Name = "Command crew capacity";
        Description = "Crew capacity of all parts.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = null;
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.TotalCommandCrewCapacity;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class CommNetRange : VesselEntry
{
    public CommNetRange()
    {
        Name = "CommNet Range";
        Description = "Maximum CommNet range.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        MiliUnit = "m";
        BaseUnit = "km";
        KiloUnit = "Mm";
        MegaUnit = "Gm";
        GigaUnit = "Tm";
        NumberOfDecimalDigits = 0;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.SimulationObject.Telemetry.CommNetRangeMeters / 1000f;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class CommNetStatus : VesselEntry
{
    public CommNetStatus()
    {
        Name = "CommNet Status";
        Description = "Status of the CommNet connection.";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = null;
        Formatting = null;
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.SimulationObject.Telemetry.CommNetConnectionStatus;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class NonStageableResourcesEntry : VesselEntry
{
    public NonStageableResourcesEntry()
    {
        Name = "Non-stageable resources";
        Description = "Display a list of all non-stageable resources on the vessel, their values and capacity.";
        EntryType = EntryType.NonStageableResources;
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = null;
        NumberOfDecimalDigits = 2;
        Formatting = "N";
        AltUnit = new AltUnit()
        {
            IsActive = false,
            Unit = string.Empty, //not used, will be handled in NonStageableResourcesEntriesBuilder
            Factor = 1 //not used, will be handled in NonStageableResourcesEntriesBuilder
        };
    }

    public override void RefreshData()
    {
        EntryValue = ParseNonstageableResources(Utility.ActiveVessel.SimulationObject?.PartOwner?.ContainerGroup.GetAllResourcesContainedData().ToList());
    }

    private List<NonStageableResource> ParseNonstageableResources(List<ContainedResourceData> resources)
    {
        var parsedResources = new List<NonStageableResource>();
        
        if (resources != null && !resources.Any())
            return parsedResources;

        foreach (var resource in resources)
        {
            var resourceDefinition =  GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID);
            if (resourceDefinition.resourceProperties.NonStageable)
            {
                parsedResources.Add(new NonStageableResource
                {
                    Name = resourceDefinition.DisplayName,
                    CapacityUnits = resource.CapacityUnits,
                    StoredUnits = resource.StoredUnits,
                    
                    // I'm sorry for this :(
                    Unit = resourceDefinition.name switch
                    {
                        "ElectricCharge" => Units.SymbolUnits,
                        "Water" => "L",
                        _ => Units.SymbolTonne
                    }
                });
            }
        }

        return parsedResources;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class AngularSpeed : VesselEntry
{
    public AngularSpeed()
    {
        Name = "Angular Speed";
        Description = "";
        Category = MicroEntryCategory.Vessel;
        IsDefault = false;
        BaseUnit = "rad/s";
        NumberOfDecimalDigits = 2;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue =
            Utility.ActiveVessel.mainBody.celestialMotionFrame.ToLocalAngularVelocity(Utility.ActiveVessel
                .AngularVelocity).magnitude;
    }

    public override string ValueDisplay => base.ValueDisplay;
}