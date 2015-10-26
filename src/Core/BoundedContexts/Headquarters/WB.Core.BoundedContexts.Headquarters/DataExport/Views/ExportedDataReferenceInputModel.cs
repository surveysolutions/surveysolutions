using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferenceInputModel
    {
        public ExportedDataReferenceInputModel() { }

        public ExportedDataReferenceInputModel(Guid questionnaireId, long questionnaireVersion)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
    }
}