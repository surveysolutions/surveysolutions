using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public interface IQuestionnaireBrowseViewFactory 
    {
        QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input);
        QuestionnaireBrowseItem GetById(QuestionnaireIdentity identity);
    }
}