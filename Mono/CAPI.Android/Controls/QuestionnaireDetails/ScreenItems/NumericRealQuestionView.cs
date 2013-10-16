using System;
using System.Globalization;
using Android.Content;
using Android.Text;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericRealQuestionView : NumericQuestionView<decimal>
    {
        public NumericRealQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override InputTypes KeyboardTypeFlags
        {
            get { return InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned; }
        }

       
        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out decimal answer)
        {
            var replacedAnswer = newAnswer.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

            return decimal.TryParse(replacedAnswer, out answer);
        }

        protected override void SaveAnswer(string newAnswer, decimal answer)
        {
            this.SaveAnswer(newAnswer, new AnswerNumericRealQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer));
        }
    }
}