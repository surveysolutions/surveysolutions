using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IQuestionnaireBrowseViewFactory 
    {
        QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input);
        QuestionnaireBrowseItem GetById(QuestionnaireIdentity identity);
    }
}