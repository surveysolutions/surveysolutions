using System;
using Android.Content;
using WB.Core.BoundedContexts.CAPI.Views.InterviewDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IQuestionViewFactory
    {
         AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model, Guid questionnairePublicKey);
        // AbstractQuestionView CreateQuestionView(Context context, AbstractQuestionRowItem model, HeaderItem header);
    }
}