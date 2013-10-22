using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IAnswerOnQuestionCommandService
    {
        void AnswerOnQuestion(AnswerQuestionCommand command, Action<Exception> errorCallback);
    }
}