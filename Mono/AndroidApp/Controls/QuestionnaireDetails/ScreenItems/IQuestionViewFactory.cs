using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IQuestionViewFactory
    {
         AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model);
         AbstractQuestionView CreateQuestionView(Context context, AbstractQuestionRowItem model, HeaderItem header);
    }
}