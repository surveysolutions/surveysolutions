namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class GpsCoordinatesViewModel
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public GpsCoordinatesViewModel(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}