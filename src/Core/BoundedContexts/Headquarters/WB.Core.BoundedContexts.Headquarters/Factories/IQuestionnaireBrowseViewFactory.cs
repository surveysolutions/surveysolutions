using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IQuestionnaireBrowseViewFactory 
    {
        QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input);
        QuestionnaireBrowseItem GetById(QuestionnaireIdentity identity);
        List<QuestionnaireBrowseItem> GetByIds(params QuestionnaireIdentity[] identities);
        IEnumerable<QuestionnaireIdentity> GetAllQuestionnaireIdentities();
    }
}
