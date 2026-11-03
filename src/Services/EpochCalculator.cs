using System;

namespace GpsDengelemeApp.Services
{
    /// <summary>
    /// Tarihten ondalıklı epok değeri hesaplar.
    /// Örnek: 15 Temmuz 2023 → 2023.5342
    /// </summary>
    public static class EpochCalculator
    {
        /// <summary>
        /// Verilen tarih için ondalıklı epok değerini döndürür.
        /// </summary>
        public static double ToDecimalEpoch(DateTime date)
        {
            int year = date.Year;
            DateTime startOfYear = new DateTime(year, 1, 1);
            DateTime endOfYear   = new DateTime(year + 1, 1, 1);

            double totalDaysInYear = (endOfYear - startOfYear).TotalDays;
            double dayOfYear       = (date - startOfYear).TotalDays;

            return Math.Round(year + (dayOfYear / totalDaysInYear), 4);
        }
    }
}
