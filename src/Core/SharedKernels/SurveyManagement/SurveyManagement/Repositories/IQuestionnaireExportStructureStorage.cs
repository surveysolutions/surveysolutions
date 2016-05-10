using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Repositories
{
    public interface IQuestionnaireExportStructureStorage
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id);
    }
}