using System;
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IQuestionViewFactory
    {
         AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model, Guid questionnairePublicKey);
        // AbstractQuestionView CreateQuestionView(Context context, AbstractQuestionRowItem model, HeaderItem header);
    }
}