using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class ImportDataParsingErrorsView
    {
        public ImportDataParsingErrorsView(
            QuestionnaireIdentity identity,
            string questionnaireTitle,
            PanelImportVerificationError[] errors,
            string fileName = null)
        {
            this.FileName = fileName;
            this.Identity = identity;
            this.Errors = errors;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public QuestionnaireIdentity Identity { get; }

        public PanelImportVerificationError[] Errors { get; }

        public string FileName { get; set; }

        public string QuestionnaireTitle { get; set; }
    }
}
