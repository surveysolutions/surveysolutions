using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class DataExportArchive
    {
        public Stream Data { get; set; }
        public string Redirect { get; set; }
        public string FileName { get; set; }
    }
}
