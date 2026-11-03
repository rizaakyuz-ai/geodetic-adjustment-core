using System;
using GpsDengelemeApp.Services;

namespace GpsDengelemeApp.Models
{
    public class BazKontrolPair
    {
        public string StationName { get; set; }
        public string TargetBaseName { get; set; }
        
        public BaselineObservation MainObservation { get; set; }
        public BaselineObservation ControlObservation { get; set; }

        public bool IsComplete => MainObservation != null && ControlObservation != null;

        // Differences
        public double dX => Math.Abs(MainObservation.DX - ControlObservation.DX);
        public double dY => Math.Abs(MainObservation.DY - ControlObservation.DY);
        public double dZ => Math.Abs(MainObservation.DZ - ControlObservation.DZ);

        // Distance in km from Main Observation
        public double DistanceKm => Math.Sqrt(
            MainObservation.DX * MainObservation.DX + 
            MainObservation.DY * MainObservation.DY + 
            MainObservation.DZ * MainObservation.DZ) / 1000.0;

        // Criterion (30mm + 3ppm for C3)
        public double CriterionM => 0.030 + (3e-6 * DistanceKm * 1000.0);
    }
}
