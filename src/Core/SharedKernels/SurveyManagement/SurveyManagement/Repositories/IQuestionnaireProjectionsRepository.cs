using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Repositories
{
    public interface IQuestionnaireProjectionsRepository
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id);
    }
}