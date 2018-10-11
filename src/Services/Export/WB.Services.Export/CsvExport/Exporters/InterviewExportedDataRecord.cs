using System.Collections.Generic;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewExportedDataRecord
    {
        public virtual string InterviewId { get; set; }
        public virtual Dictionary<string, string[]> Data { get; set; } // file name, rows
    }
}