using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GpsDengelemeApp.Services
{
    /// <summary>
    /// TG20 / TG09 jeoid yükseklik hizmeti.
    /// Desteklenen formatlar:
    ///   1) Nokta Adı + N  →  LoadPointNameFile()   ← EN KOLAY
    ///   2) lat lon N listesi veya ASCII grid başlığı  →  LoadGrid()
    /// Yükleme yapılmazsa FallbackN sabit değeri kullanılır.
    /// </summary>
    public class GeoidService
    {
        // ── 1) Nokta Adı Bazlı Sözlük ────────────────────────────────────
        private readonly Dictionary<string, double> _pointN
            = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Nokta adı eşleştirmeli dosya yüklü mü?</summary>
        public bool IsPointFileLoaded => _pointN.Count > 0;

        /// <summary>Yüklenen nokta-N dosyasının yolu (bilgi amaçlı).</summary>
        public string PointFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Nokta Adı + N değeri formatında dosya yükler.
        ///
        /// Desteklenen format (başlık isteğe bağlı, # = yorum):
        ///   # Nokta   N(m)
        ///   P001      35.412
        ///   P002      36.100
        ///
        /// Ayraç: boşluk / sekme / virgül / noktalı virgül
        /// </summary>
        public void LoadPointNameFile(string filePath)
        {
            _pointN.Clear();

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split(
                    new[] { ' ', '\t', ',', ';' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2 &&
                    double.TryParse(parts[1], NumberStyles.Float,
                                    CultureInfo.InvariantCulture, out double n))
                {
                    _pointN[parts[0]] = n;
                }
            }

            if (_pointN.Count == 0)
                throw new FormatException("Dosyada geçerli 'Nokta Adı  N' satırı bulunamadı.");

            PointFilePath = filePath;
        }

        /// <summary>
        /// Nokta adına göre N değeri döndürür.
        /// Eşleşme bulunamazsa → koordinat bazlı grid → FallbackN sırasıyla denenir.
        /// </summary>
        public double GetUndulationByName(string pointName, double latDeg, double lonDeg)
        {
            if (_pointN.TryGetValue(pointName, out double n))
                return n;

            return GetUndulation(latDeg, lonDeg);   // grid veya sabit
        }

        // ── 2) Grid / Nokta Listesi ───────────────────────────────────────
        private double[,]? _grid;
        private double _latMin, _latMax, _lonMin, _lonMax, _latStep, _lonStep;
        private int _rows, _cols;

        public bool   IsGridLoaded => _grid is not null;
        public string GridFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Sabit yedek N değeri (grid/liste yüklü değilse).
        /// Türkiye için makul ortalama ≈ 35 m.
        /// </summary>
        public double FallbackN { get; set; } = 35.0;

        /// <summary>
        /// ASCII grid (başlık + matris) veya lat lon N nokta listesi yükler.
        /// </summary>
        public void LoadGrid(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) throw new FormatException("Dosya boş.");

            var header = lines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (header.Length >= 6 && TryParseGridHeader(header, out var meta))
                LoadGridMatrix(lines, meta);
            else
                LoadPointList(lines);

            GridFilePath = filePath;
        }

        private bool TryParseGridHeader(string[] h,
            out (double latMin, double latMax, double lonMin, double lonMax, double dLat, double dLon) meta)
        {
            meta = default;
            try
            {
                meta = (double.Parse(h[0], CultureInfo.InvariantCulture),
                        double.Parse(h[1], CultureInfo.InvariantCulture),
                        double.Parse(h[2], CultureInfo.InvariantCulture),
                        double.Parse(h[3], CultureInfo.InvariantCulture),
                        double.Parse(h[4], CultureInfo.InvariantCulture),
                        double.Parse(h[5], CultureInfo.InvariantCulture));
                return true;
            }
            catch { return false; }
        }

        private void LoadGridMatrix(string[] lines,
            (double latMin, double latMax, double lonMin, double lonMax, double dLat, double dLon) m)
        {
            _latMin = m.latMin; _latMax = m.latMax;
            _lonMin = m.lonMin; _lonMax = m.lonMax;
            _latStep = m.dLat;  _lonStep = m.dLon;
            _rows = (int)Math.Round((_latMax - _latMin) / _latStep) + 1;
            _cols = (int)Math.Round((_lonMax - _lonMin) / _lonStep) + 1;
            _grid = new double[_rows, _cols];

            int r = 0, c = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                foreach (var tok in lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (r >= _rows) break;
                    _grid[r, c] = double.Parse(tok, CultureInfo.InvariantCulture);
                    if (++c >= _cols) { c = 0; r++; }
                }
            }
        }

        private void LoadPointList(string[] lines)
        {
            var pts = new List<(double lat, double lon, double n)>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;
                var p = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length >= 3)
                    pts.Add((double.Parse(p[0], CultureInfo.InvariantCulture),
                             double.Parse(p[1], CultureInfo.InvariantCulture),
                             double.Parse(p[2], CultureInfo.InvariantCulture)));
            }
            if (pts.Count == 0) throw new FormatException("Grid dosyasında geçerli veri bulunamadı.");

            pts.Sort((a, b) => a.lat != b.lat ? a.lat.CompareTo(b.lat) : a.lon.CompareTo(b.lon));

            var lats = new SortedSet<double>(); var lons = new SortedSet<double>();
            foreach (var pt in pts) { lats.Add(pt.lat); lons.Add(pt.lon); }
            var latList = new List<double>(lats);
            var lonList = new List<double>(lons);

            _latMin = latList[0]; _latMax = latList[^1];
            _lonMin = lonList[0]; _lonMax = lonList[^1];
            _latStep = latList.Count > 1 ? latList[1] - latList[0] : 0.01;
            _lonStep = lonList.Count > 1 ? lonList[1] - lonList[0] : 0.01;
            _rows = latList.Count; _cols = lonList.Count;
            _grid = new double[_rows, _cols];

            foreach (var pt in pts)
            {
                int ri = (int)Math.Round((pt.lat - _latMin) / _latStep);
                int ci = (int)Math.Round((pt.lon - _lonMin) / _lonStep);
                if (ri >= 0 && ri < _rows && ci >= 0 && ci < _cols)
                    _grid[ri, ci] = pt.n;
            }
        }

        /// <summary>Koordinat bazlı bilinear interpolasyon ile N döndürür.</summary>
        public double GetUndulation(double latDeg, double lonDeg)
        {
            if (_grid is null) return FallbackN;
            if (latDeg < _latMin || latDeg > _latMax || lonDeg < _lonMin || lonDeg > _lonMax)
                return FallbackN;

            double ri = (latDeg - _latMin) / _latStep;
            double ci = (lonDeg - _lonMin) / _lonStep;
            int r0 = (int)ri, c0 = (int)ci;
            int r1 = Math.Min(r0 + 1, _rows - 1);
            int c1 = Math.Min(c0 + 1, _cols - 1);
            double dr = ri - r0, dc = ci - c0;

            return (1 - dr) * (1 - dc) * _grid[r0, c0]
                 + (1 - dr) *      dc  * _grid[r0, c1]
                 +      dr  * (1 - dc) * _grid[r1, c0]
                 +      dr  *      dc  * _grid[r1, c1];
        }
    }
}
