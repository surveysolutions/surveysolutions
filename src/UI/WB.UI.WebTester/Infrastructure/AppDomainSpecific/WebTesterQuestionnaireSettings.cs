using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.WebTester.Infrastructure.AppDomainSpecific;

public class WebTesterQuestionnaireSettings : IQuestionnaireSettings
{
    public CriticalityLevel? GetCriticalityLevel(QuestionnaireIdentity identity)
    {
        return CriticalityLevel.Block;
    }
}
