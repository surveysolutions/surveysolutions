namespace WB.Core.SharedKernels.DataCollection
{
    public class GeoLocation
    {
        public GeoLocation(double latitude, double longitude, double? accuracy, double? altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Altitude = altitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
    }
}
