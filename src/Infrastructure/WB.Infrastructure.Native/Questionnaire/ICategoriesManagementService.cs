using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.Questionnaire
{
    public interface ICategoriesManagementService
    {
        List<CategoriesInstance> GetAll(QuestionnaireIdentity questionnaireId);
        void Delete(QuestionnaireIdentity questionnaireId);
        void Store(IEnumerable<CategoriesInstance> translationInstances);
    }
}
