using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ApprovedDataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public ApprovedDataExportProcessDetails(string processName, DataExportFormat format, QuestionnaireIdentity questionnaire)
            : base(processName, format)
        {
            this.Questionnaire = questionnaire;
        }

        public QuestionnaireIdentity Questionnaire { get; }
    }
}