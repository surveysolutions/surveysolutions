using System;
using Android.Content;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public interface IQuestionViewFactory
    {
         AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model, Guid questionnairePublicKey);
        // AbstractQuestionView CreateQuestionView(Context context, AbstractQuestionRowItem model, HeaderItem header);
    }
}