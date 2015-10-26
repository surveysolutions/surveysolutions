using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    public class DataExportProcessDto
    {
        public virtual string DataExportProcessId { get; set; }
        public virtual DateTime BeginDate { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual DataExportType DataExportType { get; set; }
        public virtual DataExportStatus Status { get; set; }
        public virtual int ProgressInPercents { get; set; }
    }
}