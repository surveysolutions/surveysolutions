using System;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
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

        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out int answer)
        {
            return int.TryParse(newAnswer, out answer);
        }

        protected override AnswerQuestionCommand CreateAnswerQuestionCommand(int answer)
        {
            return new AnswerNumericIntegerQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer);
        }
    }
}