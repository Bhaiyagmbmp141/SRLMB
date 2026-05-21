using System;
using System.Collections.Generic;

namespace SRLMB.Civil3D.Helpers
{
    /// <summary>
    /// Manning's equation for circular pipes flowing full.
    /// Q = (1/n) * A * R^(2/3) * S^(1/2)
    /// </summary>
    internal static class Manning
    {
        // Roughness coefficients (n) for common pipe materials
        private static readonly Dictionary<string, double> NValues =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "Concrete",             0.013 },
                { "Reinforced Concrete",  0.013 },
                { "RCP",                  0.013 },
                { "HDPE",                 0.009 },
                { "PVC",                  0.009 },
                { "UPVC",                 0.009 },
                { "Ductile Iron",         0.012 },
                { "Cast Iron",            0.013 },
                { "Steel",                0.011 },
                { "Clay",                 0.013 },
                { "Vitrified Clay",       0.012 },
                { "Corrugated Metal",     0.024 },
                { "CMP",                  0.024 },
            };

        public static double NForMaterial(string? material)
        {
            if (material != null && NValues.TryGetValue(material, out double n))
                return n;
            return 0.013; // default: concrete
        }

        /// <param name="diameterM">Inner diameter in metres.</param>
        /// <param name="slopeMM">Slope in m/m (not %).</param>
        /// <param name="n">Manning's roughness coefficient.</param>
        /// <returns>Full-flow capacity in m³/s.</returns>
        public static double FullFlowCapacity(double diameterM, double slopeMM, double n)
        {
            if (diameterM <= 0 || slopeMM <= 0 || n <= 0) return 0;
            double area = Math.PI * diameterM * diameterM / 4.0;
            double r    = diameterM / 4.0; // hydraulic radius for full circle = D/4
            return (1.0 / n) * area * Math.Pow(r, 2.0 / 3.0) * Math.Sqrt(slopeMM);
        }

        // Standard nominal diameters (mm) in ascending order for recommending next size
        public static readonly double[] StandardSizesMm =
        {
            100, 150, 200, 225, 250, 300, 375, 450,
            525, 600, 675, 750, 825, 900, 1050, 1200, 1350, 1500
        };

        /// <summary>
        /// Returns the smallest standard diameter (metres) whose full-flow capacity
        /// meets or exceeds <paramref name="requiredM3s"/> at the given slope.
        /// </summary>
        public static double RecommendDiameter(double requiredM3s, double slopeMM, double n)
        {
            foreach (double sizeMm in StandardSizesMm)
            {
                double d = sizeMm / 1000.0;
                if (FullFlowCapacity(d, slopeMM, n) >= requiredM3s)
                    return d;
            }
            return StandardSizesMm[StandardSizesMm.Length - 1] / 1000.0;
        }
    }
}
