using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class DataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public DataExportProcessDetails(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle)
            : base(format)
        {
            this.Questionnaire = questionnaire;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public QuestionnaireIdentity Questionnaire { get; }
        public string QuestionnaireTitle { get; }

        public override string NaturalId => $"{InterviewStatusString()}${this.Format}${this.Questionnaire}";

        public override string Name => $"(ver. {this.Questionnaire.Version}) {this.QuestionnaireTitle}";

        public InterviewStatus? InterviewStatus { get; set; }

        private string InterviewStatusString()
        {
            return InterviewStatus?.ToString() ?? "All";
        }
    }
}