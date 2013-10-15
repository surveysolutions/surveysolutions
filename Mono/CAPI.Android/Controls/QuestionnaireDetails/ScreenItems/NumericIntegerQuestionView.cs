using System;
using Android.Content;
using Android.Text;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericIntegerQuestionView : NumericQuestionView
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

        protected override void ParseAndSaveAnswer(string newAnswer)
        {
            int answer;
            if (!int.TryParse(newAnswer, out  answer))
                return;
            if (!IsCommentsEditorFocused)
                HideKeyboard(etAnswer);

            this.SaveAnswer(newAnswer, new AnswerNumericIntegerQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer));
        }
    }
}