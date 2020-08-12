using System;

namespace WB.UI.WebTester.Services
{
    public class MultimediaFile
    {
        public MultimediaFile(string filename, byte[] data, TimeSpan? duration, string mimeType)
        {
            Filename = filename;
            Data = data;
            Duration = duration;
            MimeType = mimeType;
        }

        public string Filename { get; set; }
        public byte[] Data { get; set; }
        public TimeSpan? Duration { get; set; }

        public string MimeType { get; set; }
    }
}
