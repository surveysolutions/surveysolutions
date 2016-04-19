using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IOptionsRepository
    {
        IReadOnlyList<CategoricalQuestionOption> GetQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId);
        Task RemoveOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireId);
        Task StoreQuestionOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument serializedQuestionnaireDocument);
    }
}