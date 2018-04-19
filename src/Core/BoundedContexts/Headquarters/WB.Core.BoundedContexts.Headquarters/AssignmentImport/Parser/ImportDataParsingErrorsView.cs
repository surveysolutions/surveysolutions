using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class ImportDataParsingErrorsView
    {
        public static ImportDataParsingErrorsView CreatePrerequisiteError(
            Guid questionnaireId,
            long version,
            string questionnaireTitle,
            string error,
            string fileName = null)
            => new ImportDataParsingErrorsView(
                questionnaireId,
                version,
                questionnaireTitle,
                new[] { new PanelImportVerificationError("PL0000", error) },
                new InterviewImportError[0],
                false,
                fileName);

        public ImportDataParsingErrorsView(
            Guid questionnaireId, 
            long version, 
            string questionnaireTitle,
            PanelImportVerificationError[] errors,
            InterviewImportError[] importErrors,
            bool wasSupervisorProvided, 
            string fileName = null)
        {
            this.FileName = fileName;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Errors = errors;
            this.ImportErrors = importErrors;
            this.WasSupervisorProvided = wasSupervisorProvided;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PanelImportVerificationError[] Errors { get; private set; }

        public InterviewImportError[] ImportErrors { get; private set; }

        public string FileName { get; set; }

        public bool WasSupervisorProvided { get; set; }
        public string QuestionnaireTitle { get; set; }
    }
}
