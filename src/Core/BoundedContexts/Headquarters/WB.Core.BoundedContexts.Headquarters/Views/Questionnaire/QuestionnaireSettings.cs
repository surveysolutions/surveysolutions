using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

public class QuestionnaireSettings : IQuestionnaireSettings
{
    private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

    public QuestionnaireSettings(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
    {
        this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
    }
    
    public CriticalityLevel? GetCriticalityLevel(QuestionnaireIdentity identity)
    {
        var questionnaireBrowseItem = questionnaireBrowseItemStorage.GetById(identity.ToString());
        return questionnaireBrowseItem.CriticalityLevel;
    }
}
