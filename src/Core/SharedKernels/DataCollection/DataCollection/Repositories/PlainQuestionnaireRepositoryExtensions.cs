using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public static class PlainQuestionnaireRepositoryExtensions
    {
        public static IQuestionnaire GetQuestionnaire(this IPlainQuestionnaireRepository repository, QuestionnaireIdentity identity)
            => repository.GetHistoricalQuestionnaire(identity.QuestionnaireId, identity.Version);
    }
}