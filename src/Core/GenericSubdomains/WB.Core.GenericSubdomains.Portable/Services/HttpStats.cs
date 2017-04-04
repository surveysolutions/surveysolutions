using System;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public class HttpStats
    {
        public long Downloaded { get; set; }
        public long Uploaded { get; set; }
        public TimeSpan Duration { get; set; }

        public double Speed => this.Duration.TotalSeconds > 0 ? (this.Downloaded + this.Uploaded) / this.Duration.TotalSeconds : 0;
    }
}