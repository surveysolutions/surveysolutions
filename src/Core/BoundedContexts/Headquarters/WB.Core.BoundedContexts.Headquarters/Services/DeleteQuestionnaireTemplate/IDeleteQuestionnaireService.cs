using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    public interface IDeleteQuestionnaireService
    {
        Task DisableQuestionnaire(Guid questionnaireId,
            long questionnaireVersion, Guid userId);

        Task DeleteInterviewsAndQuestionnaireAfterAsync(Guid questionnaireId, long questionnaireVersion, Guid userId);
    }
}
