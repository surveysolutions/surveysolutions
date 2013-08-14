using System;
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class GeoPositionQuestionView : AbstractQuestionView
    {
        public GeoPositionQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}