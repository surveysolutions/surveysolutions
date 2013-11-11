using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public interface IAnswerOnQuestionCommandService
    {
        void AnswerOnQuestion(AnswerQuestionCommand command, Action<Exception> errorCallback);
    }
}