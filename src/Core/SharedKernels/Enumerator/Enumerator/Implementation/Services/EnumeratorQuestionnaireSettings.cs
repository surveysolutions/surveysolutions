using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services;

public class EnumeratorQuestionnaireSettings : IQuestionnaireSettings
{
    private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;

    public EnumeratorQuestionnaireSettings(IPlainStorage<QuestionnaireView> questionnaireViewRepository)
    {
        this.questionnaireViewRepository = questionnaireViewRepository;
    }
    
    public CriticalityLevel? GetCriticalityLevel(QuestionnaireIdentity identity)
    {
        var questionnaire = questionnaireViewRepository.GetById(identity.ToString());
        return questionnaire.CriticalityLevel;
    }
}
