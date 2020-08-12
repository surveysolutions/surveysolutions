using System;
using System.IO;

namespace WB.Services.Export.Jobs
{
    public class DataExportArchive
    {
        public Stream? Data { get; set; }
        public string Redirect { get; set; } = String.Empty;
        public string FileName { get; set; } = String.Empty;
    }
}
