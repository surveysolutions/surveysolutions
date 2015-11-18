using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ApprovedDataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public ApprovedDataExportProcessDetails(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle)
            : base(format)
        {
            this.Questionnaire = questionnaire;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public QuestionnaireIdentity Questionnaire { get; }
        public string QuestionnaireTitle { get; }

        public override string NaturalId => $"Approved${this.Format}${this.Questionnaire}";

        public override string Name => $"(ver. {this.Questionnaire.Version}) {this.QuestionnaireTitle}, Approved";
    }
}