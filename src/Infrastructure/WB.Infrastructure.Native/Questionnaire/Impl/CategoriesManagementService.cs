using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.Questionnaire.Impl
{
    public class CategoriesManagementService : ICategoriesManagementService
    {
        private readonly IPlainStorageAccessor<CategoriesInstance> categoriesStorage;

        public CategoriesManagementService(IPlainStorageAccessor<CategoriesInstance> categoriesStorage)
        {
            this.categoriesStorage = categoriesStorage;
        }

        public List<CategoriesInstance> GetAll(QuestionnaireIdentity questionnaireId) =>
            this.categoriesStorage.Query(_ =>
                _.Where(x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                             x.QuestionnaireId.Version == questionnaireId.Version).ToList());

        public void Delete(QuestionnaireIdentity questionnaireId) =>
            this.categoriesStorage.Remove(_ => _.Where(x =>
                x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                x.QuestionnaireId.Version == questionnaireId.Version));

        public void Store(IEnumerable<CategoriesInstance> instances) => this.categoriesStorage.Store(instances);
    }
}
