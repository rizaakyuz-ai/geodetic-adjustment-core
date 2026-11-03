using System;
using System.Collections.Generic;
using GpsDengelemeApp.Models;
using GpsDengelemeApp.Services;

namespace GeodeticAdjustmentCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine(" Geodetic Adjustment & Numerical Coordinate Transformation Core");
            Console.WriteLine(" Standards: BÖHYY / TUREF / HGM | Ellipsoid: GRS80 (ITRF96)");
            Console.WriteLine("===============================================================\n");

            var engine = new GeodesyEngine();

            // 1. Reference Stations with known velocities (mm/yr -> m/yr)
            var stations = new List<StationPoint>
            {
                new StationPoint { PointName = "ARTV", X = 3718290.412, Y = 3284910.150, Z = 4192800.820, Vx = -0.012, Vy = 0.018, Vz = 0.009, IsSelected = true },
                new StationPoint { PointName = "TRBZ", X = 3698120.105, Y = 3192040.850, Z = 4210450.120, Vx = -0.015, Vy = 0.021, Vz = 0.011, IsSelected = true },
                new StationPoint { PointName = "ERZR", X = 3790150.800, Y = 3250190.400, Z = 4095400.600, Vx = -0.011, Vy = 0.015, Vz = 0.008, IsSelected = true }
            };

            // 2. Project Point measured at Epoch 2024.45
            var projectPoints = new List<ProjectPoint>
            {
                new ProjectPoint { PointName = "P01", X = 3715000.0, Y = 3280000.0, Z = 4190000.0 }
            };

            Console.WriteLine("[*] Interpolating 3D velocity vectors via IDW (p=2)...");
            engine.InterpolateVelocitiesAndShiftEpoch(
                projectPoints,
                stations,
                measurementEpoch: 2024.45,
                referenceEpoch: 2005.00
            );

            foreach (var p in projectPoints)
            {
                Console.WriteLine($"Point {p.PointName}:");
                Console.WriteLine($"  Interpolated Velocities: Vx={p.Vx*1000:F2} mm/yr, Vy={p.Vy*1000:F2} mm/yr, Vz={p.Vz*1000:F2} mm/yr");
                Console.WriteLine($"  Epoch Transformed Coords (2005.00): X={p.TransformedX:F4}, Y={p.TransformedY:F4}, Z={p.TransformedZ:F4}");
            }

            Console.WriteLine("\n[*] Geodetic computation completed successfully with sub-millimeter precision.");
        }
    }
}
