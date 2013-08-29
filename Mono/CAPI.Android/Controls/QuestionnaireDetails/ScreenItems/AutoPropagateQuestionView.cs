using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class AutoPropagateQuestionView : NumericQuestionView
    {
        public AutoPropagateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            etAnswer.InputType = InputTypes.ClassNumber;
        }
    }
}