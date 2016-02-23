using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Repositories
{
    public interface IQuestionnaireProjectionsRepository
    {
        ReferenceInfoForLinkedQuestions GetReferenceInfoForLinkedQuestions(QuestionnaireIdentity identity);
        QuestionnaireRosterStructure GetQuestionnaireRosterStructure(QuestionnaireIdentity identity);
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity identity);
        QuestionnaireQuestionsInfo GetQuestionnaireQuestionsInfo(QuestionnaireIdentity identity);
    }
}