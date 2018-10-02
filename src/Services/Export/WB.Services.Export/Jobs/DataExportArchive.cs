using System;
using System.IO;

namespace WB.Services.Export.Jobs
{
    public class DataExportArchive
    {
        public Stream Data { get; set; }
        public Uri Redirect { get; set; }
        public string FileName { get; set; }
    }
}