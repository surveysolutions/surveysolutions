using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal class QuestionnaireVersionProvider : IQuestionnaireVersionProvider
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireVersionProvider(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public long GetNextVersion(Guid id)
        {
            var availableVersions =
                this.questionnaireBrowseItemStorage.Query(
                    _ => _.Where(q => q.QuestionnaireId == id).Select(q => q.Version));

            if (!availableVersions.Any())
                return 1;

            return availableVersions.Max() + 1;
        }

        public long? GetPreviousVersion(Guid id, long version)
        {
            var availableVersions =
                this.questionnaireBrowseItemStorage.Query(
                    _ => _.Where(q => q.QuestionnaireId == id && q.Version < version && !q.IsDeleted)
                        .Select(q => q.Version)
                        .ToList());

            return availableVersions.Count == 0 ? null : availableVersions.Max();
        }
    }
}