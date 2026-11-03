namespace GpsDengelemeApp.Models
{
    /// <summary>
    /// Proje noktası — tam hesaplama sonuçlarını barındırır.
    /// BÖHYY: Metrik koordinatlar 4 basamak, DMS saniyesi 5 basamak.
    /// </summary>
    public class ProjectPoint
    {
        public string PointName { get; set; } = string.Empty;

        // ── Ham ITRF koordinatları (ölçüm epoğu) ────────────────────────
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        // ── IDW ile kestirilmiş hız vektörleri (m/yıl, 4 basamak) ──────
        public double CalculatedVx { get; set; }
        public double CalculatedVy { get; set; }
        public double CalculatedVz { get; set; }

        // ── Referans epoğa dönüştürülmüş ITRF XYZ (4 basamak) ──────────
        public double TransformedX { get; set; }
        public double TransformedY { get; set; }
        public double TransformedZ { get; set; }

        // ── Dengeleme / düzeltme koordinatları (isteğe bağlı) ───────────
        public double AdjustedX { get; set; }
        public double AdjustedY { get; set; }
        public double AdjustedZ { get; set; }

        // ── Coğrafi koordinatlar (DMS string + ondalık derece) ──────────
        /// <summary>Örnek: 41° 27' 11.86710" N</summary>
        public string LatitudeDMS  { get; set; } = string.Empty;
        /// <summary>Örnek: 41° 53' 29.31941" E</summary>
        public string LongitudeDMS { get; set; } = string.Empty;

        // Ondalık derece (hesaplamalar için)
        public double LatitudeDec  { get; set; }
        public double LongitudeDec { get; set; }

        // ── Elipsoit yüksekliği (h, 4 basamak) ─────────────────────────
        public double EllipsoidHeight { get; set; }

        // ── TUREF TM 3° Projeksiyon (4 basamak) ─────────────────────────
        /// <summary>Sağa Değer Y = y + 500 000 m</summary>
        public double GridEastY  { get; set; }
        /// <summary>Yukarı Değer X</summary>
        public double GridNorthX { get; set; }

        // ── Jeoid & Ortometrik Yükseklik (4 basamak) ────────────────────
        public double GeoidN          { get; set; }
        public double OrthometricH    { get; set; }
    }
}
