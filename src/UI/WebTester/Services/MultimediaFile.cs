using System;

namespace WB.UI.WebTester.Services
{
    public class MultimediaFile
    {
        public string Filename { get; set; }
        public byte[] Data { get; set; }
        public TimeSpan? Duration { get; set; }

        public string MimeType { get; set; }
    }
}