using System;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public class HttpStats
    {
        public long DownloadedBytes { get; set; }
        public long UploadedBytes { get; set; }
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Connection speed in bytes per second
        /// </summary>
        public double Speed => this.Duration.TotalSeconds > 0 ? (this.DownloadedBytes + this.UploadedBytes) / this.Duration.TotalSeconds : 0;
    }
}