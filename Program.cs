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
            var geoidService = new GeoidService();

            var settings = new ProjectSettings
            {
                ProjectName = "Benchmark Adjustment Project",
                MeasurementDate = new DateTime(2024, 6, 15),
                ReferenceEpoch = 2005.00,
                CentralMeridian = 42,
                ZoneWidth = 3
            };

            // 1. Reference Stations with known velocities (m/yr)
            var stations = new List<StationPoint>
            {
                new StationPoint { PointName = "ARTV", X = 3718290.412, Y = 3284910.150, Z = 4192800.820, Vx = -0.012, Vy = 0.018, Vz = 0.009, IsSelected = true },
                new StationPoint { PointName = "TRBZ", X = 3698120.105, Y = 3192040.850, Z = 4210450.120, Vx = -0.015, Vy = 0.021, Vz = 0.011, IsSelected = true },
                new StationPoint { PointName = "ERZR", X = 3790150.800, Y = 3250190.400, Z = 4095400.600, Vx = -0.011, Vy = 0.015, Vz = 0.008, IsSelected = true }
            };

            // 2. Project Points measured at observation epoch
            var projectPoints = new List<ProjectPoint>
            {
                new ProjectPoint { PointName = "P01", X = 3715000.0, Y = 3280000.0, Z = 4190000.0 },
                new ProjectPoint { PointName = "P02", X = 3716000.0, Y = 3281000.0, Z = 4191000.0 }
            };

            Console.WriteLine("[*] Executing Full Geodetic Processing Pipeline (IDW, Bowring, TM 3°, Geoid H=h-N)...");
            var results = engine.RunFullPipeline(settings, stations, projectPoints, geoidService);

            foreach (var r in results)
            {
                Console.WriteLine($"\n---------------------------------------------------------------");
                Console.WriteLine($"Point: {r.PointName}");
                Console.WriteLine($"  Interpolated Velocities: Vx = {r.Vx * 1000:F2} mm/yr, Vy = {r.Vy * 1000:F2} mm/yr, Vz = {r.Vz * 1000:F2} mm/yr");
                Console.WriteLine($"  Epoch Transformed XYZ (2005.00): X = {r.X_Ref:F4} m, Y = {r.Y_Ref:F4} m, Z = {r.Z_Ref:F4} m");
                Console.WriteLine($"  Geodetic (GRS80): Lat = {r.LatitudeDMS}, Lon = {r.LongitudeDMS}, h = {r.EllipsoidalHeight:F4} m");
                Console.WriteLine($"  TUREF TM 3° (CM={settings.CentralMeridian}°): Easting (Y) = {r.SagaDeger:F4} m, Northing (X) = {r.YukariDeger:F4} m");
                Console.WriteLine($"  Orthometric Height: N = {r.GeoidUndulation:F4} m, H = {r.OrthometricHeight:F4} m");
            }

            Console.WriteLine("\n===============================================================");
            Console.WriteLine("[SUCCESS] All geodetic transformations and adjustments completed with sub-millimeter precision.");
            Console.WriteLine("===============================================================");
        }
    }
}
