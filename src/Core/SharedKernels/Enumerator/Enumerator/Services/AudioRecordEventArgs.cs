using System;
using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public class AudioRecordEventArgs : EventArgs
    {
        public Stream Source { get; }
        public TimeSpan Duration { get; }
        public string ContentType { get; }

        public AudioRecordEventArgs(Stream source, TimeSpan duration, string contentType)
        {
            this.Source = source;
            this.Duration = duration;
            this.ContentType = contentType;
        }
    }
}