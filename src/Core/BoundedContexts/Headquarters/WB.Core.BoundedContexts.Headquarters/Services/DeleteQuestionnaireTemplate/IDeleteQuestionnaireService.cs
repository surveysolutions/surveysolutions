using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    public interface IDeleteQuestionnaireService
    {
        void DisableQuestionnaire(Guid questionnaireId,
            long questionnaireVersion, Guid? userId);

        void DeleteInterviewsAndQuestionnaireAfter(Guid questionnaireId, long questionnaireVersion, Guid? userId);
    }
}
