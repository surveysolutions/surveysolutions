using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    public interface IDeleteQuestionnaireService
    {
        Task DeleteQuestionnaire(Guid questionnaireId,
            long questionnaireVersion, Guid? userId);
    }
}
