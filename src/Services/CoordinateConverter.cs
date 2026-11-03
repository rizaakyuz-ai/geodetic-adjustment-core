using System;

namespace GpsDengelemeApp.Services
{
    /// <summary>
    /// Koordinat dönüşüm ve formatlama yardımcıları.
    /// BÖHYY: Metrik 4 basamak, DMS saniye 5 basamak.
    /// </summary>
    public static class CoordinateConverter
    {
        // ─────────────────────────────────────────────────────────────────
        // Ondalık Derece → DMS Formatı
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ondalık derece değerini DMS formatına çevirir.
        /// Çıktı:  41° 27' 11.86710" N  (saniye: 5 basamak)
        /// </summary>
        public static string ToDMS(double decimalDegrees, bool isLatitude)
        {
            double abs     = Math.Abs(decimalDegrees);
            int    degrees = (int)abs;
            double remMin  = (abs - degrees) * 60.0;
            int    minutes = (int)remMin;
            double seconds = (remMin - minutes) * 60.0;

            // Saniyeyi 5 basamağa yuvarla — taşma kontrolü
            seconds = Math.Round(seconds, 5, MidpointRounding.AwayFromZero);
            if (seconds >= 60.0) { seconds = 0.0; minutes++; }
            if (minutes >= 60)   { minutes = 0;   degrees++; }

            string dir = isLatitude
                ? (decimalDegrees >= 0 ? "N" : "S")
                : (decimalDegrees >= 0 ? "E" : "W");

            return $"{degrees}° {minutes}' {seconds:F5}\" {dir}";
        }

        // ─────────────────────────────────────────────────────────────────
        // BÖHYY 4 Basamak Yuvarlama
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Metrik koordinatı BÖHYY standardına uygun 4 basamağa yuvarlar.
        /// MidpointRounding.AwayFromZero kullanılır (örn: 0.00005 → 0.0001).
        /// </summary>
        public static double Round4(double value)
            => Math.Round(value, 4, MidpointRounding.AwayFromZero);

        /// <summary>Hız değerini 4 basamağa yuvarlar (m/yıl).</summary>
        public static double Round4V(double value)
            => Math.Round(value, 7, MidpointRounding.AwayFromZero); // hızlar 7 basamak

        // ─────────────────────────────────────────────────────────────────
        // DMS → Ondalık Derece (Tersine Çevrim)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>DMS string'den ondalık dereceye çevirir.</summary>
        public static double FromDMS(int deg, int min, double sec, bool negative = false)
        {
            double d = deg + min / 60.0 + sec / 3600.0;
            return negative ? -d : d;
        }
    }
}
