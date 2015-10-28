using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Dtos
{
    public class ExportedDataReferenceDto
    {
        public virtual string ExportedDataReferenceId { get; set; }
        public virtual Guid? QuestionnaireId { get; set; }
        public virtual long? QuestionnaireVersion { get; set; }
        public virtual DataExportFormat DataExportFormat { get; set; }
        public virtual DataExportType DataExportType { get; set; }
        public virtual string ExportedDataPath { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime FinishDate { get; set; }
        public virtual string DataExportProcessId { get; set; }
    }
}