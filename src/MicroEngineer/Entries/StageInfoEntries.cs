using KSP.Sim.DeltaV;
using MicroEngineer.Utilities;

namespace MicroEngineer.Entries;

public class StageInfoEntry: BaseEntry
{ }

public class StageInfo : StageInfoEntry
{
    public StageInfo()
    {
        Name = "Stage Info";
        Description = "";
        EntryType = EntryType.StageInfo;
        Category = MicroEntryCategory.Stage;
        IsDefault = true;
        BaseUnit = null;
        Formatting = null;
    }

    public override void RefreshData()
    {
        EntryValue = ParseStages(Utility.ActiveVessel.VesselDeltaV?.StageInfo);
    }

    public override string ValueDisplay => base.ValueDisplay;

    public List<Stage> ParseStages(List<DeltaVStageInfo> deltaVStages)
    {
        var stages = new List<Stage>();

        if (deltaVStages == null)
            return stages;

        var nonEmptyStages = new List<DeltaVStageInfo>();
        foreach (var stage in deltaVStages)
        {
            if (stage.DeltaVinVac > 0.0001 || stage.DeltaVatASL > 0.0001)
            {
                nonEmptyStages.Add(stage);
            }
        }
        
        for (int i = nonEmptyStages.Count - 1; i >= 0; i--)
        {
            var time = Utility.ParseSecondsToTimeFormat(nonEmptyStages[i].StageBurnTime);
            var stage = new Stage
            {
                StageNumber = deltaVStages.Count - nonEmptyStages[i].Stage,
                DeltaVActual = nonEmptyStages[i].DeltaVActual,
                TwrActual = nonEmptyStages[i].TWRActual,
                BurnDays = time.Days,
                BurnHours = time.Hours,
                BurnMinutes = time.Minutes,
                BurnSeconds = time.Seconds
            };
            stages.Add(stage);
        }

        return stages;
    }
}

public class StageFuelPercentage : StageInfoEntry
{
    public StageFuelPercentage()
    {
        Name = "Stage Fuel %";
        Description = "Stage fuel percentage left.";
        Category = MicroEntryCategory.Stage;
        IsDefault = false;
        BaseUnit = "%";
        NumberOfDecimalDigits = 1;
        Formatting = "N";
    }

    public override void RefreshData()
    {
        EntryValue = Utility.ActiveVessel.StageFuelPercentage;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageDryMass : StageInfoEntry
{
    public StageDryMass()
    {
        Name = "Stage Dry Mass";
        Description = "Dry mass of the current stage (will be decoupled on staging).";
        Category = MicroEntryCategory.Stage;
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
        var stageInfo = Utility.ActiveVessel.VesselDeltaV?.StageInfo;
        if (stageInfo == null || stageInfo.Count == 0)
        {
            EntryValue = null;
            return;
        }

        var stageDryMass = stageInfo[0].DryMass;

        // if a next stage exists then subtract its drymass from the current drymass to get current stage drymass 
        if (stageInfo.Count >= 2)
        {
            stageDryMass -= stageInfo[1].DryMass;
        }
        
        // multiply by 100 to convert from tons to kg
        EntryValue = stageDryMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageFuelMass : StageInfoEntry
{
    public StageFuelMass()
    {
        Name = "Stage Fuel Mass";
        Description = "Remaining fuel mass of the current stage.";
        Category = MicroEntryCategory.Stage;
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
        var stage = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault();
        if (stage == null)
        {
            EntryValue = null;
            return;
        }

        EntryValue = stage.EndMass != 0f ? (stage.StartMass - stage.EndMass) * 1000f : 0f;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class StageEndMass : StageInfoEntry
{
    public StageEndMass()
    {
        Name = "Stage End Mass";
        Description = "End mass of the current stage.";
        Category = MicroEntryCategory.Stage;
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
        var stage = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault();
        if (stage == null)
        {
            EntryValue = null;
            return;
        }

        EntryValue = stage.EndMass == 0 ? stage.StartMass * 1000 : stage.EndMass == stage.StartMass ? null: stage.EndMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}

public class DecoupledMass : StageInfoEntry
{
    public DecoupledMass()
    {
        Name = "Decoupled Mass";
        Description = "Decoupled mass of the current stage.";
        Category = MicroEntryCategory.Stage;
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
        EntryValue = Utility.ActiveVessel.VesselDeltaV?.StageInfo.FirstOrDefault()?.DecoupledMass * 1000;
    }

    public override string ValueDisplay => base.ValueDisplay;
}