using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class GeoLocationPoint
    {
        public GeoLocationPoint(double latitude, double longitude, double accuracy, DateTimeOffset timestamp)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
        }

        public DateTimeOffset Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }

    }
}
