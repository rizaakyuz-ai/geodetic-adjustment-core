using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GpsDengelemeApp.Models
{
    /// <summary>
    /// ITRF sabit istasyon noktası.
    /// Epok dönüşümünde kullanılır — tüm koordinatlar 4 basamak hassasiyetle saklanır.
    /// IsSelected: Kullanıcının IDW hız hesabında baz alınacak referans istasyon seçimi.
    /// </summary>
    public class StationPoint : INotifyPropertyChanged
    {
        // ── Referans Seçim Kutucuğu (BÖHHBÜY: kullanıcı seçimi zorunlu) ──
        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string PointName { get; set; } = string.Empty;

        // Ham ITRF koordinatları (ölçüm epoğu)
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        // Hız bileşenleri (m/yıl)
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Vz { get; set; }

        // Hesaplanan 2005.00 epok koordinatları — BÖHYY: 4 basamak hassasiyet
        public double TransformedX { get; set; }
        public double TransformedY { get; set; }
        public double TransformedZ { get; set; }

        public bool IsTransformed =>
            TransformedX != 0 || TransformedY != 0 || TransformedZ != 0;

        // ── INotifyPropertyChanged ─────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
