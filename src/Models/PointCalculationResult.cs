namespace GpsDengelemeApp.Models
{
    /// <summary>
    /// Bir proje noktasına ait tüm hesaplama sonuçlarını tutar.
    /// BÖHYY / HGM / TUREF standartlarına uygun.
    /// </summary>
    public class PointCalculationResult
    {
        public string PointName { get; set; } = string.Empty;

        // ── 1. Kartezyen Koordinatlar (Ölçüm Epoğu — ECEF XYZ) ──────────
        public double X_Meas { get; set; }
        public double Y_Meas { get; set; }
        public double Z_Meas { get; set; }

        // ── 2. Hız Vektörleri (IDW ile Kestirilmiş — m/yıl) ─────────────
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Vz { get; set; }

        // ── 3. Kartezyen Koordinatlar (Referans Epoğu 2005.00 — ECEF XYZ)
        public double X_Ref { get; set; }
        public double Y_Ref { get; set; }
        public double Z_Ref { get; set; }

        // ── 4. Coğrafi Koordinatlar (GRS80 Elipsoidi) ───────────────────
        public double Latitude           { get; set; }  // φ — Enlem  (derece ondalık)
        public double Longitude          { get; set; }  // λ — Boylam (derece ondalık)
        public double EllipsoidalHeight  { get; set; }  // h — Elipsoit yüksekliği (m)

        // DMS formatı — BÖHYY: saniye 5 basamak
        // Örnek: 41° 27' 11.86710" N
        public string LatitudeDMS  { get; set; } = string.Empty;
        public string LongitudeDMS { get; set; } = string.Empty;

        // ── 5. TUREF / TM 3° Düzlem Koordinatları ───────────────────────
        public double SagaDeger   { get; set; }  // Y + 500 000 m (Sağa Değer)
        public double YukariDeger { get; set; }  // X (Yukarı Değer — m)

        // ── 6. Jeoid Yüksekliği & Ortometrik Yükseklik ──────────────────
        public double GeoidUndulation    { get; set; }  // N (m) — TG20 kestirimi
        public double OrthometricHeight  { get; set; }  // H = h − N (m)
    }
}
