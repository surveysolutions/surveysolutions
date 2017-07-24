using System;
using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public class AudioRecordEventArgs : EventArgs
    {
        public Stream Source { get; }
        public TimeSpan Duration { get; }
        public string ContentType { get; }
        public string Type { get; }

        public AudioRecordEventArgs(Stream source, TimeSpan duration, string contentType, string type)
        {
            this.Source = source;
            this.Duration = duration;
            this.ContentType = contentType;
            this.Type = type;
        }
    }
}