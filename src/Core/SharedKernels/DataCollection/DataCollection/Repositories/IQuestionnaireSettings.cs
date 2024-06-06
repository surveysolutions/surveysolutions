#nullable enable
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Repositories;

public interface IQuestionnaireSettings
{
    CriticalityLevel? GetCriticalityLevel(QuestionnaireIdentity identity);
}
