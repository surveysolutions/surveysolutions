using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate
{
    public interface IDeleteQuestionnaireService
    {
        void DeleteQuestionnaire(Guid questionnaireId,
            long questionnaireVersion, Guid? userId);
    }
}
