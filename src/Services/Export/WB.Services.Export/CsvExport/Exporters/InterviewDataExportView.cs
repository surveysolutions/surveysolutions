using System;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewDataExportView 
    {
        public InterviewDataExportView(Guid interviewId,InterviewDataExportLevelView[] levels, int errorsCount)
        {
            this.InterviewId = interviewId;
            this.Levels = levels;
            this.ErrorsCount = errorsCount;
        }

        public Guid InterviewId { get; set; }

        public InterviewDataExportLevelView[] Levels { get; set; }
        public int ErrorsCount { get; }
    }
}
