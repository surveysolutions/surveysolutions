using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class RunningDataExportProcessView
    {
        public RunningDataExportProcessView(string dataExportProcessId, DateTime beginDate, DateTime lastUpdateDate, string questionnaireTitle, long? questionnaireVersion, int progress, DataExportType type, DataExportFormat format)
        {
            this.DataExportProcessId = dataExportProcessId;
            this.BeginDate = beginDate;
            this.LastUpdateDate = lastUpdateDate;
            this.QuestionnaireTitle = questionnaireTitle;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Progress = progress;
            this.Type = type;
            this.Format = format;
        }

        public string DataExportProcessId { get; private set; }
        public DateTime BeginDate { get; private set; }
        public DateTime LastUpdateDate { get; private set; }
        public string QuestionnaireTitle { get; private set; }
        public long? QuestionnaireVersion { get; private set; }
        public int Progress { get; private set; }
        public DataExportType Type { get; private set; }
        public DataExportFormat Format { get; private set; }
    }
}