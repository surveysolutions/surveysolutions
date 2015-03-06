using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public interface IQuestionnaireBrowseViewFactory 
    {
        QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input);
    }
}