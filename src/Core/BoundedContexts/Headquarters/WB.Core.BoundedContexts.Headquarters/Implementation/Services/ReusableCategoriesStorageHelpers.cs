using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public static class ReusableCategoriesStorageHelpers
    {
        public static void Store(this IPlainKeyValueStorage<QuestionnaireReusableCategories> storage, QuestionnaireReusableCategories lookupTable, QuestionnaireIdentity questionnaire, Guid categoriesId)
        {
            storage.Store(lookupTable, storage.GetReusableCategoriesKey(questionnaire, categoriesId));
        }

        public static QuestionnaireReusableCategories Get(this IPlainKeyValueStorage<QuestionnaireReusableCategories> storage, QuestionnaireIdentity questionnaire, Guid categoriesId)
        {
            return storage.GetById(storage.GetReusableCategoriesKey(questionnaire, categoriesId));
        }

        public static string GetReusableCategoriesKey(this IPlainKeyValueStorage<QuestionnaireReusableCategories> storage, QuestionnaireIdentity questionnaireIdentity, Guid categoriesId)
        {
            return questionnaireIdentity + "-" + categoriesId.FormatGuid();
        }
    }
}
