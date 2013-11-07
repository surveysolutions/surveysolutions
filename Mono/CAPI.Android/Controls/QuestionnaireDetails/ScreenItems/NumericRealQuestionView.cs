using System;
using System.Globalization;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.CAPI.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

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


        protected override void Initialize()
        {
            base.Initialize();
            this.AttachDecimalPlacesFilterToEditTextIfNeeded();
        }

        private void AttachDecimalPlacesFilterToEditTextIfNeeded()
        {
            var valueModel = this.Model as ValueQuestionViewModel;

            if (valueModel == null)
                throw new InvalidCastException("Something bad happened with mapping question's viewmodel to question's view");

            if (valueModel.CountOfDecimalPlaces.HasValue)
                this.etAnswer.SetFilters(new IInputFilter[] { new DecimalPlacesFilter(valueModel.CountOfDecimalPlaces.Value) });
        }

        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out decimal answer)
        {
            var replacedAnswer = newAnswer.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

            return decimal.TryParse(replacedAnswer, out answer);
        }

        protected override AnswerQuestionCommand CreateAnswerQuestionCommand(decimal answer)
        {
            return new AnswerNumericRealQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer);
        }
    }
}