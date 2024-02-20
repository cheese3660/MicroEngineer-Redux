using KSP.Api;
using KSP.Sim.impl;

namespace MicroEngineer.Utilities;

public static class OrbitExtensions
{
    public static double NextClosestApproachTime(
        this PatchedConicsOrbit a,
        PatchedConicsOrbit b,
        double UT)
    {
        double num1 = UT;
        double num2 = double.MaxValue;
        double num3 = UT;
        double num4 = a.period;
        if (a.eccentricity > 1.0)
            num4 = 100.0 / a.MeanMotion();
        double num5 = UT + num4;
        for (int index1 = 0; index1 < 8; ++index1)
        {
            double num6 = (num5 - num3) / 20.0;
            for (int index2 = 0; index2 < 20; ++index2)
            {
                double UT1 = num3 + (double) index2 * num6;
                double num7 = a.Separation(b, UT1);
                if (num7 < num2)
                {
                    num2 = num7;
                    num1 = UT1;
                }
            }
            num3 = Math.Clamp(num1 - num6, UT, UT + num4);
            num5 = Math.Clamp(num1 + num6, UT, UT + num4);
        }
        return num1;
    }
    
    public static double MeanMotion(this PatchedConicsOrbit o)
    {
        return o.eccentricity > 1.0 ? Math.Sqrt(o.referenceBody.gravParameter / Math.Abs(Math.Pow(o.semiMajorAxis, 3.0))) : 2.0 * Math.PI / o.period;
    }
    
    public static double Separation(this PatchedConicsOrbit a, PatchedConicsOrbit b, double UT)
    {
        return (a.WorldPositionAtUT(UT) - b.WorldPositionAtUT(UT)).magnitude;
    }
    
    public static Vector3d WorldPositionAtUT(this PatchedConicsOrbit o, double UT)
    {
        return o.referenceBody.transform.celestialFrame.ToLocalPosition((ICoordinateSystem) o.ReferenceFrame, o.referenceBody.Position.localPosition + o.GetRelativePositionAtUTZup(UT).SwapYAndZ);
    }
    
    public static double NextClosestApproachDistance(
        this PatchedConicsOrbit a,
        PatchedConicsOrbit b,
        double UT)
    {
        return a.Separation(b, a.NextClosestApproachTime(b, UT));
    }
    
    // Not working correctly
    // public static double RelativeSpeed(this PatchedConicsOrbit a, PatchedConicsOrbit b, double UT)
    // {
    //     return Vector3d.Dot(a.WorldOrbitalVelocityAtUT(UT) - b.WorldOrbitalVelocityAtUT(UT), (a.WorldBCIPositionAtUT(UT) - b.WorldBCIPositionAtUT(UT)).normalized);
    // }
    
    public static Vector3d WorldOrbitalVelocityAtUT(this PatchedConicsOrbit o, double UT)
    {
        return o.referenceBody.transform.celestialFrame.ToLocalPosition((ICoordinateSystem) o.ReferenceFrame, o.GetOrbitalVelocityAtUTZup(UT).SwapYAndZ);
    }
    
    public static Vector3d WorldBCIPositionAtUT(this PatchedConicsOrbit o, double UT)
    {
        return o.referenceBody.transform.celestialFrame.ToLocalPosition((ICoordinateSystem) o.ReferenceFrame, o.GetRelativePositionAtUTZup(UT).SwapYAndZ);
    }
}