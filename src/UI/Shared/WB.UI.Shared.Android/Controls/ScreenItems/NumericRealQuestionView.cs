using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class NumericRealQuestionView : NumericQuestionView<decimal>
    {
        public NumericRealQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override InputTypes KeyboardTypeFlags
        {
            get { return InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned; }
        }

        protected override string FormatString(string s)
        {
            if (s.EndsWith(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator))
                return s;

            if (s.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator) && s.EndsWith("0"))
                return s;

            decimal parsedAnswer;
            if (!IsParseAnswerStringSucceeded(s, out parsedAnswer))
                return s;

            return QuestionUtils.FormatDecimalAnswer(parsedAnswer);
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
                throw new InvalidCastException("Something bad happened with mapping question's view model to question's view");

            if (valueModel.CountOfDecimalPlaces.HasValue)
                this.etAnswer.SetFilters(new IInputFilter[] { new DecimalPlacesFilter(valueModel.CountOfDecimalPlaces.Value) });
        }

        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out decimal answer)
        {
            var replacedAnswer = newAnswer.Replace(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return decimal.TryParse(replacedAnswer, NumberStyles.Number, CultureInfo.CurrentCulture, out answer);
        }

        protected override Task<AnswerQuestionCommand> CreateAnswerQuestionCommand(decimal answer)
        {
            return Task.Run<AnswerQuestionCommand>(() => new AnswerNumericRealQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id,
                this.Model.PublicKey.InterviewItemPropagationVector,
                DateTime.UtcNow, answer));
        }
    }
}