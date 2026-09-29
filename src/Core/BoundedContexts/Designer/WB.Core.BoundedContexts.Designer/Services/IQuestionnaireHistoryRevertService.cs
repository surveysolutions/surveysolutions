using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireHistoryRevertService
    {
        Task<bool> TryRevertAsync(
            Guid questionnaireId,
            QuestionnaireChangeRecord historicalRecord,
            Guid responsibleId,
            string responsibleName);
    }
}
