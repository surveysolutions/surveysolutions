using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class NumericIntegerQuestionView : NumericQuestionView<int>
    {
        public NumericIntegerQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership) {}

        protected override InputTypes KeyboardTypeFlags
        {
            get { return InputTypes.ClassNumber | InputTypes.NumberFlagSigned; }
        }

        protected override string FormatString(string s)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:n0}", 
                int.Parse(s, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture));
        }

        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out int answer)
        {
            return int.TryParse(newAnswer, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out answer);
        }

        protected override async Task<AnswerQuestionCommand> CreateAnswerQuestionCommand(int answer)
        {
            bool result = await ConfirmRosterDecreaseAsync(Model.TriggeredRosters, ParseCurrentAnswerString() - answer);

            if (!result)
            {
                return null;
            }

            return new AnswerNumericIntegerQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id,
                this.Model.PublicKey.InterviewItemPropagationVector,
                DateTime.UtcNow, answer);
        }

        private int ParseCurrentAnswerString()
        {
            int result;

            if (IsParseAnswerStringSucceeded(Model.AnswerString, out result))
                return result;

            return 0;
        }
    }
}