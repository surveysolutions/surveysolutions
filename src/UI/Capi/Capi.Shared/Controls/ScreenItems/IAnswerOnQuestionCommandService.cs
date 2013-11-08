using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public interface IAnswerOnQuestionCommandService
    {
        void AnswerOnQuestion(AnswerQuestionCommand command, Action<Exception> errorCallback);
    }
}