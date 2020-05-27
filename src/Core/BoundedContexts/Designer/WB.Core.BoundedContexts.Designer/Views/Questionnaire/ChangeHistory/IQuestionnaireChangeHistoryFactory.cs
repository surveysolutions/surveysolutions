using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public interface IQuestionnaireChangeHistoryFactory
    {
        Task<QuestionnaireChangeHistory?> LoadAsync(Guid id, int page, int pageSize, IPrincipal user);
    }
}
