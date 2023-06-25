﻿using BepInEx.Logging;
using KSP.Game;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;

namespace MicroMod
{
    public class OabStageInfoEntry : BaseEntry
    { }

    public class TotalBurnTime_OAB : OabStageInfoEntry
    {
        public bool UseDHMSFormatting; // TODO: implement

        public TotalBurnTime_OAB()
        {
            Name = "Total burn time (OAB)";
            Description = "Shows the total length of burn the vessel can mantain.";
            EntryType = EntryType.Time;            
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            BaseUnit = "s";
            NumberOfDecimalDigits = 1;
            Formatting = "N";
            UseDHMSFormatting = true;
        }

        public override void RefreshData()
        {
            EntryValue = Utility.VesselDeltaVComponentOAB?.TotalBurnTime;
        }

        public override string ValueDisplay
        {
            get
            {
                if (EntryValue == null)
                    return "-";

                if (UseDHMSFormatting)
                    return Utility.SecondsToTimeString((double)EntryValue);
                else
                    return String.IsNullOrEmpty(this.Formatting) ? EntryValue.ToString() : String.Format(Formatting, EntryValue);
            }
        }
    }

    public class TotalDeltaVASL_OAB : OabStageInfoEntry
    {
        public TotalDeltaVASL_OAB()
        {
            Name = "Total ∆v ASL (OAB)";
            Description = "Shows the vessel's total delta velocity At Sea Level.";
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            MiliUnit = "mm/s";
            BaseUnit = "m/s";
            KiloUnit = "km/s";
            MegaUnit = "Mm/s";
            GigaUnit = "Gm/s";
            NumberOfDecimalDigits = 0;
            Formatting = "N";
        }
        public override void RefreshData()
        {
            EntryValue = Utility.VesselDeltaVComponentOAB?.TotalDeltaVASL;
        }

        public override string ValueDisplay => base.ValueDisplay;
    }

    public class TotalDeltaVActual_OAB : OabStageInfoEntry
    {
        public TotalDeltaVActual_OAB()
        {
            Name = "Total ∆v Actual (OAB)";
            Description = "Shows the vessel's actual total delta velocity (not used in OAB).";
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            MiliUnit = "mm/s";
            BaseUnit = "m/s";
            KiloUnit = "km/s";
            MegaUnit = "Mm/s";
            GigaUnit = "Gm/s";
            NumberOfDecimalDigits = 0;
            Formatting = "N";
        }
        public override void RefreshData()
        {
            EntryValue = Utility.VesselDeltaVComponentOAB?.TotalDeltaVActual;
        }

        public override string ValueDisplay => base.ValueDisplay;
    }

    public class TotalDeltaVVac_OAB : OabStageInfoEntry
    {
        public TotalDeltaVVac_OAB()
        {
            Name = "Total ∆v Vac (OAB)";
            Description = "Shows the vessel's total delta velocity in Vacuum.";
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            MiliUnit = "mm/s";
            BaseUnit = "m/s";
            KiloUnit = "km/s";
            MegaUnit = "Mm/s";
            GigaUnit = "Gm/s";
            NumberOfDecimalDigits = 0;
            Formatting = "N";
        }
        public override void RefreshData()
        {
            EntryValue = Utility.VesselDeltaVComponentOAB?.TotalDeltaVVac;
        }

        public override string ValueDisplay => base.ValueDisplay;
    }

    /// <summary>
    /// Calculates torque from the Center of Thrust and Center of Mass
    /// </summary>    
    public class Torque : OabStageInfoEntry
    {
        public Torque()
        {
            Name = "Torque";
            Description = "Thrust torque that is generated by not having Thrust Vector and Center of Mass aligned. Turn on the Center of Thrust and Center of Mass VAB indicators to get an accurate value.";
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            BaseUnit = null;
            Formatting = null;
        }

        public override void RefreshData()
        {
            Vector3d com = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.CenterOfProperties?.CoM ?? Vector3d.zero;
            Vector3d cot = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.CenterOfProperties?.CoT ?? Vector3d.zero;

            if (com == Vector3d.zero || cot == Vector3d.zero)
                return;

            List<DeltaVEngineInfo> engines = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.VesselDeltaV?.EngineInfo;
            if (engines == null || engines.Count == 0)
                return;

            Vector3d force = new Vector3d();

            foreach (var engine in engines)
            {
                force += engine.ThrustVectorVac;
            }

            var leverArm = cot - com;

            Vector3d torque = Vector3d.Cross(force, (Vector3d)leverArm);

            this.EntryValue = torque.magnitude;
            this.BaseUnit = (double)EntryValue >= 1.0 ? "kNm" : "Nm";
        }

        public override string ValueDisplay
        {
            get
            {
                if (EntryValue == null)
                    return "-";

                if ((double)EntryValue >= 1.0)
                    return $"{String.Format("{0:F2}", EntryValue)}";

                return Math.Abs((double)EntryValue) > double.Epsilon ? $"{String.Format("{0:F2}", (double)EntryValue * 1000.0)}" : $"{String.Format("{0:F0}", (double)EntryValue)}";
            }
        }
    }

    /// <summary>
    /// Holds stage info parameters for each stage. Also keeps information about the celestial body user selected in the window.
    /// </summary>
    public class StageInfo_OAB : OabStageInfoEntry
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("MicroEngineer.StageInfo_OAB");
        public List<CelestialBody> CelestialBodyForStage = new();
        public delegate void StageInfoOABChanged(List<DeltaVStageInfo_OAB> stages);
        public event StageInfoOABChanged OnStageInfoOABChanged;

        public StageInfo_OAB()
        {
            Name = "Stage Info (OAB)";
            Description = "Holds a list of stage info parameters.";
            EntryType = EntryType.StageInfoOAB;
            Category = MicroEntryCategory.OAB;
            IsDefault = true;
            BaseUnit = null;
            Formatting = null;
        }

        public override void RefreshData()
        {
            _logger.LogDebug("Entering RefreshData.");
            EntryValue ??= new List<DeltaVStageInfo_OAB>();

            ((List<DeltaVStageInfo_OAB>)EntryValue).Clear();

            if (Utility.VesselDeltaVComponentOAB?.StageInfo == null) return;

            for (int i = 0; i < Utility.VesselDeltaVComponentOAB.StageInfo.Count; i++)            
            {
                var retrieved = Utility.VesselDeltaVComponentOAB.StageInfo[i];
                var stage = new DeltaVStageInfo_OAB()
                {
                    Index = i,
                    DeltaVActual = retrieved.DeltaVActual,
                    DeltaVASL = retrieved.DeltaVatASL,
                    DeltaVVac = retrieved.DeltaVinVac,
                    DryMass = retrieved.DryMass,
                    EndMass = retrieved.EndMass,
                    FuelMass = retrieved.FuelMass,
                    IspASL = retrieved.IspASL,
                    IspActual = retrieved.IspActual,
                    IspVac = retrieved.IspVac,
                    SeparationIndex = retrieved.SeparationIndex,
                    Stage = retrieved.Stage,
                    StageBurnTime = Utility.ParseSecondsToTimeFormat(retrieved.StageBurnTime),
                    StageMass = retrieved.StageMass,
                    StartMass = retrieved.StartMass,
                    TWRASL = retrieved.TWRASL,
                    TWRActual = retrieved.TWRActual,
                    TWRVac = retrieved.TWRVac,
                    ThrustASL = retrieved.ThrustASL,
                    ThrustActual = retrieved.ThrustActual,
                    ThrustVac = retrieved.ThrustVac,
                    TotalExhaustVelocityASL = retrieved.TotalExhaustVelocityASL,
                    TotalExhaustVelocityActual = retrieved.TotalExhaustVelocityActual,
                    TotalExhaustVelocityVAC = retrieved.TotalExhaustVelocityVAC,
                    DeltaVStageInfo = retrieved
                };

                if (i < CelestialBodyForStage.Count)
                {
                    // CelestialBody already created for this stage. Attaching it to the stage.
                    stage.CelestialBody = CelestialBodyForStage[i];
                }
                else
                {
                    // This is a new stage. Need to create another CelestialBody.
                    var cel = MicroCelestialBodies.Instance.GetHomeBody();
                    stage.CelestialBody = cel;
                    CelestialBodyForStage.Add(cel);
                }

                // Apply TWR factor to the value and recalculate sea level TWR and DeltaV
                stage.Recalculate_TWR();
                stage.Recalculate_SLT();
                stage.Recalculate_DeltaVSeaLevel();
                
                // KSP2 has a strange way of displaying stage numbers, so we need a little hack
                stage.Recalculate_StageNumber(Utility.VesselDeltaVComponentOAB.StageInfo.Count);

                ((List<DeltaVStageInfo_OAB>)EntryValue).Add(stage);

                OnStageInfoOABChanged?.Invoke(((List<DeltaVStageInfo_OAB>)EntryValue).Where(s => s.DeltaVVac > 0.0001 || s.DeltaVASL > 0.0001).ToList());
            }
        }

        public override string ValueDisplay
        {
            get
            {
                return "-";
            }
        }

        public void UpdateCelestialBodyAtIndex(int index, string selectedBodyName)
        {
            var body = MicroCelestialBodies.Instance.GetBodyByName(selectedBodyName);
            CelestialBodyForStage[index] = body;
            RefreshData();
        }
    }

    /// <summary>
    /// Parameters for one stage
    /// </summary>
    public class DeltaVStageInfo_OAB
    {
        /// <summary>
        /// Index in DeltaVStageInfo_OAB is linked to the CelestialBodyForStage Index.
        /// When stages are refreshed, DeltaVStageInfo_OAB grabs the CelestialBodyForStage at the same Index.
        /// </summary>
        public int Index;
        public double DeltaVActual;
        public double DeltaVASL;
        public double DeltaVVac;
        public double DryMass;
        public double EndMass;
        public double FuelMass;
        public double IspASL;
        public double IspActual;
        public double IspVac;
        public int SeparationIndex;
        public int Stage;
        public TimeParsed StageBurnTime;
        public double StageMass;
        public double StartMass;
        public float TWRASL;
        public float TWRActual;
        public float TWRVac;
        public float ThrustASL;
        public float ThrustActual;
        public float ThrustVac;
        public float TotalExhaustVelocityASL;
        public float TotalExhaustVelocityActual;
        public float TotalExhaustVelocityVAC;
        public CelestialBody CelestialBody;
        public DeltaVStageInfo DeltaVStageInfo;

        private float GetThrustAtAltitude(double altitude, CelestialBodyComponent cel) => this.DeltaVStageInfo.EnginesActiveInStage?.Select(e => e.Engine.MaxThrustOutputAtm(atmPressure: cel.GetPressure(altitude) / 101.325))?.Sum() ?? 0;
        private double GetISPAtAltitude(double altitude, CelestialBodyComponent cel)
        {
            float sum = 0;
            foreach (DeltaVEngineInfo engInfo in this.DeltaVStageInfo.EnginesActiveInStage)
                sum += engInfo.Engine.MaxThrustOutputAtm(atmPressure: cel.GetPressure(altitude) / 101.325) /
                 engInfo.Engine.currentEngineModeData.atmosphereCurve.Evaluate((float)cel.GetPressure(altitude) / 101.325f);
            return GetThrustAtAltitude(altitude, cel) / sum;
        }
        private double GetDeltaVelAlt(double altitude, CelestialBodyComponent cel) => GetISPAtAltitude(altitude, cel) * 9.80665 * Math.Log(this.DeltaVStageInfo.StartMass / this.DeltaVStageInfo.EndMass);
        private double GetTWRAtAltitude(double altitude, CelestialBodyComponent cel) => this.DeltaVStageInfo.TWRVac * (GetThrustAtAltitude(altitude, cel) / this.DeltaVStageInfo.ThrustVac);
        private double GetTWRAtSeaLevel(CelestialBodyComponent cel) => this.GetTWRAtAltitude(0, cel);
        private double GetDeltaVelAtSeaLevel(CelestialBodyComponent cel) => GetDeltaVelAlt(0, cel);

        // Public methods to be used to recalculate retrieved values
        public void Recalculate_TWR() => this.TWRVac = this.TWRVac * (float)this.CelestialBody.TwrFactor;
        public void Recalculate_SLT() => this.TWRASL = (float)(this.GetTWRAtSeaLevel(this.CelestialBody.CelestialBodyComponent) * this.CelestialBody.TwrFactor);
        public void Recalculate_DeltaVSeaLevel() => this.DeltaVASL = this.GetDeltaVelAtSeaLevel(this.CelestialBody.CelestialBodyComponent);
        public void Recalculate_StageNumber(int noOfStages) => this.Stage = noOfStages - this.Stage;
    }
}
