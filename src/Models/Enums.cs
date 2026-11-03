namespace GpsDengelemeApp.Models
{
    /// <summary>
    /// Dengeleme modülü türlerini tanımlar.
    /// </summary>
    public enum AdjustmentType
    {
        Triangulation = 1, // C1-C4 Nirengi Dengelemesi
        Polygon = 2,       // Poligon Dengelemesi
        Leveling = 3       // GPS Nivelmani
    }
}
