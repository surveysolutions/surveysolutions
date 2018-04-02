using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public static class LookupStorageHelpers
    {
        public static void Store(this IPlainKeyValueStorage<QuestionnaireLookupTable> storage, QuestionnaireLookupTable lookupTable, QuestionnaireIdentity questionnaire, Guid lookupId)
        {
            storage.Store(lookupTable, storage.GetLookupKey(questionnaire, lookupId));
        }

        public static QuestionnaireLookupTable Get(this IPlainKeyValueStorage<QuestionnaireLookupTable> storage, QuestionnaireIdentity questionnaire, Guid lookupId)
        {
            return storage.GetById(storage.GetLookupKey(questionnaire, lookupId));
        }

        public static string GetLookupKey(this IPlainKeyValueStorage<QuestionnaireLookupTable> storage, QuestionnaireIdentity questionnaireIdentity, Guid lookupId)
        {
            return questionnaireIdentity + "-" + lookupId.FormatGuid();
        }
    }
}