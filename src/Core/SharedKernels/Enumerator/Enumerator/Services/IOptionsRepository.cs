using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IOptionsRepository
    {
        IReadOnlyList<CategoricalOption> GetQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId);

        IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId,
            int? parentValue, string filter);

        CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, int optionValue);

        Task RemoveOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireId);
        Task StoreQuestionOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument serializedQuestionnaireDocument);
        bool Any();
    }
}