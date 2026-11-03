using System.Collections.Generic;
using GpsDengelemeApp.Models;

namespace GpsDengelemeApp.Services
{
    /// <summary>
    /// GPS dengeleme hesaplama motoru.
    /// Epok dönüşümü ve dengeleme işlemlerini yürütür.
    /// </summary>
    public class CalculationEngine
    {
        /// <summary>
        /// Sabit istasyon koordinatlarını ölçüm epoğundan referans epoğuna (2005.00) dönüştürür.
        /// Formül: X_ref = X_ölçüm + Vx × (T_ref − T_ölçüm)
        /// </summary>
        public void ExecuteEpochTransformation(ProjectSettings settings,
                                               List<StationPoint> stations)
        {
            double measurementEpoch = EpochCalculator.ToDecimalEpoch(settings.MeasurementDate);
            double deltaT           = settings.ReferenceEpoch - measurementEpoch;

            foreach (var station in stations)
            {
                station.TransformedX = station.X + (station.Vx * deltaT);
                station.TransformedY = station.Y + (station.Vy * deltaT);
                station.TransformedZ = station.Z + (station.Vz * deltaT);
            }
        }

        /// <summary>
        /// Proje noktaları için basit dengeleme (yer tutucu — genişletilebilir).
        /// Seçili modüle göre (Nirengi / Poligon / Nivelman) farklı algoritmalar çalıştırılabilir.
        /// </summary>
        public void ExecuteAdjustment(ProjectSettings settings,
                                      List<StationPoint> stations,
                                      List<ProjectPoint> projectPoints)
        {
            // Şimdilik: proje noktalarının ham koordinatlarını "dengelenmiş" olarak kopyala.
            // Gerçek dengeleme algoritması (LSQ vb.) buraya entegre edilecek.
            foreach (var pt in projectPoints)
            {
                pt.AdjustedX = pt.X;
                pt.AdjustedY = pt.Y;
                pt.AdjustedZ = pt.Z;
            }
        }
    }
}
