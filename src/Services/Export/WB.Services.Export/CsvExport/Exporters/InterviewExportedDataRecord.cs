using System.Collections.Generic;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewExportedDataRecord
    {
        public InterviewExportedDataRecord(string interviewId, Dictionary<string, string[]> data)
        {
            InterviewId = interviewId;
            Data = data;
        }

        public virtual string InterviewId { get; set; }
        public virtual Dictionary<string, string[]> Data { get; set; } // file name, rows
    }
}
