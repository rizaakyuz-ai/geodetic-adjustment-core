using System;
using System.Collections.Generic;
using GpsDengelemeApp.Models;

namespace GpsDengelemeApp.Services
{
    /// <summary>
    /// BÖHYY / HGM / TUREF standartlarına uygun geodezik hesaplama motoru.
    /// GRS80 elipsoidi — ITRF96 / TUREF datum.
    /// </summary>
    public class GeodesyEngine
    {
        // ── GRS80 Elipsoid Sabitleri (BÖHYY Standart) ────────────────────
        public const double A   = 6_378_137.0;           // Yarı büyük eksen (m)
        public const double F   = 1.0 / 298.257_222_101; // Basıklık
        public const double E2  = 0.006_694_380_022_90;  // 1. eksantrisite²  (e²)
        private static readonly double B   = A * (1.0 - F);         // Yarı küçük eksen
        private static readonly double EP2 = (A * A - B * B) / (B * B); // 2. eksantrisite²

        // ─────────────────────────────────────────────────────────────────
        // TASK 1+2 — IDW Hız Kestirimi + Epok Kaydırma
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Her proje noktası için KULLANICININ SEÇTİĞİ (IsSelected=true) sabit
        /// istasyonların hız vektörlerinden IDW (Ters Mesafe Ağırlıklı, p=2)
        /// interpolasyon yaparak hız kestirimi yapar, ardından koordinatları
        /// ölçüm epoğundan referans epoğuna (2005.00) kaydırır.
        ///
        /// BÖHHBÜY Kural: En az 3 referans istasyon seçilmiş olmalıdır.
        /// </summary>
        public void InterpolateVelocitiesAndShiftEpoch(
            ProjectSettings              settings,
            List<StationPoint>           stations,
            List<ProjectPoint>           projectPoints,
            List<PointCalculationResult> results)
        {
            // ── BÖHHBÜY Validation: En az 3 referans istasyon ────────────
            var selectedStations = stations.Where(s => s.IsSelected).ToList();

            if (selectedStations.Count < 3)
                throw new InvalidOperationException(
                    $"Hız hesabı için en az 3 referans istasyon seçilmelidir!\n" +
                    $"Şu an seçili: {selectedStations.Count} adet.\n" +
                    $"Lütfen Adım 3'te en az 3 istasyonun checkbox'ını işaretleyin.");

            double measEpoch = EpochCalculator.ToDecimalEpoch(settings.MeasurementDate);
            double deltaT    = settings.ReferenceEpoch - measEpoch;

            results.Clear();

            foreach (var proj in projectPoints)
            {
                var res = new PointCalculationResult
                {
                    PointName = proj.PointName,
                    X_Meas    = proj.X,
                    Y_Meas    = proj.Y,
                    Z_Meas    = proj.Z
                };

                // ── IDW Hız Kestirimi — SADECE seçili istasyonlar (p=2) ──
                double sumW = 0, sumVx = 0, sumVy = 0, sumVz = 0;

                foreach (var st in selectedStations)
                {
                    double dx = proj.X - st.X;
                    double dy = proj.Y - st.Y;
                    double dz = proj.Z - st.Z;
                    double distSq = dx * dx + dy * dy + dz * dz;

                    // Eğer nokta referans istasyonun tam üstündeyse → o istasyonun hızını al
                    if (distSq < 1e-6)
                    {
                        sumW  = 1.0;
                        sumVx = st.Vx;
                        sumVy = st.Vy;
                        sumVz = st.Vz;
                        break;
                    }

                    double dist = Math.Sqrt(distSq);
                    double w = 1.0 / dist;  // BÖHYY standardı: p = 1  →  w = 1/d
                    sumVx += st.Vx * w;
                    sumVy += st.Vy * w;
                    sumVz += st.Vz * w;
                    sumW  += w;
                }

                // Hız — 7 basamak (m/yıl standardı)
                res.Vx = Math.Round(sumVx / sumW, 7, MidpointRounding.AwayFromZero);
                res.Vy = Math.Round(sumVy / sumW, 7, MidpointRounding.AwayFromZero);
                res.Vz = Math.Round(sumVz / sumW, 7, MidpointRounding.AwayFromZero);

                // ── Referans Epok Koordinatları — BÖHYY: 4 basamak ───────
                res.X_Ref = CoordinateConverter.Round4(res.X_Meas + res.Vx * deltaT);
                res.Y_Ref = CoordinateConverter.Round4(res.Y_Meas + res.Vy * deltaT);
                res.Z_Ref = CoordinateConverter.Round4(res.Z_Meas + res.Vz * deltaT);

                results.Add(res);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // TASK 4 — ECEF XYZ → Coğrafi (φ, λ, h) — Bowring Yöntemi
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kartezyen ECEF (X,Y,Z) koordinatlarını GRS80 üzerinde
        /// Coğrafi koordinatlara (Enlem, Boylam, Elipsoit h) dönüştürür.
        /// Bowring yöntemi — BÖHYY uyumlu.
        /// </summary>
        public void ConvertXYZToGeodetic(PointCalculationResult pt)
        {
            double x = pt.X_Ref, y = pt.Y_Ref, z = pt.Z_Ref;
            double p = Math.Sqrt(x * x + y * y);

            // Başlangıç tahmini (Bowring)
            double theta = Math.Atan2(z * A, p * B);

            double lat = Math.Atan2(
                z   + EP2 * B * Math.Pow(Math.Sin(theta), 3),
                p   - E2  * A * Math.Pow(Math.Cos(theta), 3));

            double lon = Math.Atan2(y, x);

            double sinLat = Math.Sin(lat);
            double N_val  = A / Math.Sqrt(1.0 - E2 * sinLat * sinLat);

            double cosLat = Math.Cos(lat);
            double h      = (Math.Abs(cosLat) > 1e-10)
                            ? p / cosLat - N_val
                            : Math.Abs(z) / Math.Abs(Math.Sin(lat)) - N_val * (1.0 - E2);

            pt.Latitude          = lat * (180.0 / Math.PI);
            pt.Longitude         = lon * (180.0 / Math.PI);
            pt.EllipsoidalHeight = h;
        }

        // ─────────────────────────────────────────────────────────────────
        // TASK 3 — Coğrafi → Gauss-Krüger / TM 3° (TUREF)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Coğrafi koordinatları (φ, λ) TUREF / ITRF96 Transverse Mercator
        /// 3° dilim projeksiyonunda Sağa (Y) ve Yukarı (X) koordinatlara çevirir.
        /// Ölçek katsayısı k₀ = 1.0 — BÖHYY standart.
        /// Sağa Değer = y + 500 000 m offset.
        /// </summary>
        public void ConvertGeodeticToTM(PointCalculationResult pt, int centralMeridianDeg)
        {
            double lat  = pt.Latitude  * (Math.PI / 180.0);
            double lon  = pt.Longitude * (Math.PI / 180.0);
            double lon0 = centralMeridianDeg * (Math.PI / 180.0);
            double l    = lon - lon0;

            double sinLat  = Math.Sin(lat);
            double cosLat  = Math.Cos(lat);
            double tanLat  = Math.Tan(lat);

            double N    = A / Math.Sqrt(1.0 - E2 * sinLat * sinLat);
            double eta2 = EP2 * cosLat * cosLat;
            double t    = tanLat;
            double t2   = t * t;

            // Meridyen yay uzunluğu (M) - Yüksek Hassasiyet (e^8'e kadar)
            double e2 = E2;
            double e4 = e2 * e2;
            double e6 = e4 * e2;
            double e8 = e4 * e4;

            double M  = A * (
                (1.0 - e2 / 4.0 - 3.0 * e4 / 64.0 - 5.0 * e6 / 256.0 - 175.0 * e8 / 16384.0) * lat -
                (3.0 * e2 / 8.0 + 3.0 * e4 / 32.0 + 45.0 * e6 / 1024.0 + 105.0 * e8 / 4096.0) * Math.Sin(2 * lat) +
                (15.0 * e4 / 256.0 + 45.0 * e6 / 1024.0 + 525.0 * e8 / 16384.0) * Math.Sin(4 * lat) -
                (35.0 * e6 / 3072.0 + 175.0 * e8 / 12288.0) * Math.Sin(6 * lat) +
                (315.0 * e8 / 131072.0) * Math.Sin(8 * lat));

            double l2 = l  * l;
            double l3 = l2 * l;
            double l4 = l3 * l;
            double l5 = l4 * l;
            double l6 = l5 * l;

            double c2 = cosLat * cosLat;
            double c3 = c2 * cosLat;
            double c5 = c3 * c2;
            double t4 = t2 * t2;

            // Yukarı Değer (X — Kuzey)
            double X_tm = M
                + N * sinLat * cosLat * l2 / 2.0
                + N * sinLat * c3 * (5.0 - t2 + 9.0 * eta2 + 4.0 * eta2 * eta2) * l4 / 24.0
                + N * sinLat * c5 * (61.0 - 58.0 * t2 + t4 + 270.0 * eta2 - 330.0 * t2 * eta2) * l6 / 720.0;

            // Sağa Değer (Y — Doğu, ham)
            double Y_tm = N * cosLat * l
                + N * c3 * (1.0 - t2 + eta2) * l3 / 6.0
                + N * c5 * (5.0 - 18.0 * t2 + t4 + 14.0 * eta2 - 58.0 * t2 * eta2) * l5 / 120.0;

            pt.YukariDeger = CoordinateConverter.Round4(X_tm);
            pt.SagaDeger   = CoordinateConverter.Round4(Y_tm + 500_000.0);
        }

        // ─────────────────────────────────────────────────────────────────
        // TASK 5 — Ortometrik Yükseklik (H = h − N)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// TG20 / TG09 veya kullanıcı tarafından sağlanan jeoid ondülasyonu N (m)
        /// değerini kullanarak H = h − N ile ortometrik yüksekliği hesaplar.
        /// </summary>
        public void CalculateOrthometricHeight(PointCalculationResult pt, double geoidN)
        {
            pt.GeoidUndulation   = geoidN;
            pt.OrthometricHeight = pt.EllipsoidalHeight - geoidN;
        }

        // ─────────────────────────────────────────────────────────────────
        // Tümünü Tek Seferde Çalıştır
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Verilen proje noktaları için tüm geodezik hesap zincirini sırayla çalıştırır:
        /// IDW hız kestirimi → epok kaydırma → XYZ→Coğrafi → TM projeksiyon → ortometrik H.
        /// </summary>
        public List<PointCalculationResult> RunFullPipeline(
            ProjectSettings    settings,
            List<StationPoint> stations,
            List<ProjectPoint> projectPoints,
            GeoidService       geoidService)
        {
            var results = new List<PointCalculationResult>();

            InterpolateVelocitiesAndShiftEpoch(settings, stations, projectPoints, results);

            foreach (var r in results)
            {
                ConvertXYZToGeodetic(r);

                // BÖHYY: DMS formatına çevir (saniye 5 basamak)
                r.LatitudeDMS  = CoordinateConverter.ToDMS(r.Latitude,  isLatitude: true);
                r.LongitudeDMS = CoordinateConverter.ToDMS(r.Longitude, isLatitude: false);
                r.EllipsoidalHeight = CoordinateConverter.Round4(r.EllipsoidalHeight);

                ConvertGeodeticToTM(r, settings.CentralMeridian);

                // Önce nokta adına göre ara, yoksa grid/sabit değer
                double N = geoidService.GetUndulationByName(r.PointName, r.Latitude, r.Longitude);
                CalculateOrthometricHeight(r, CoordinateConverter.Round4(N));
            }

            return results;
        }
    }
}
