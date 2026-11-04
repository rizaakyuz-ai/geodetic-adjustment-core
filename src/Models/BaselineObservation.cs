namespace GpsDengelemeApp.Services
{
    public class BaselineObservation
    {
        public string Station { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public double DX { get; set; }
        public double DY { get; set; }
        public double DZ { get; set; }
    }
}
