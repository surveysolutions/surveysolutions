using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAnonymousQuestionnaireStateService
    {
        Task<AnonymousQuestionnaire> SaveStateAsync(
            Guid questionnaireId,
            bool isActive,
            string questionnaireTitle,
            Guid responsibleId,
            string responsibleName);
    }
}
