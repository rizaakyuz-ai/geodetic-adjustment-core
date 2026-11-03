using System;

namespace GpsDengelemeApp.Models
{
    /// <summary>
    /// Projeye ait genel ayarları tutar.
    /// </summary>
    public class ProjectSettings
    {
        public string ProjectName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string EngineerName { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; } = DateTime.Now;

        /// <summary>Referans epok (varsayılan: 2005.00 ITRF)</summary>
        public double ReferenceEpoch { get; set; } = 2005.00;

        /// <summary>Merkezi meridyen (Artvin = 42)</summary>
        public int CentralMeridian { get; set; } = 42;

        /// <summary>Dilim genişliği: 3 veya 6 derece</summary>
        public int ZoneWidth { get; set; } = 3;

        public AdjustmentType SelectedModule { get; set; } = AdjustmentType.Triangulation;
    }
}
