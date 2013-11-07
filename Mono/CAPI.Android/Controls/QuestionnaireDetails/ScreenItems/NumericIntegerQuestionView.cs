using System;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericIntegerQuestionView : NumericQuestionView<int>
    {
        public NumericIntegerQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

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
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer);
        }
    }
}