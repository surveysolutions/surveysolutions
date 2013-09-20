using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class SingleOptionQuestionView : AbstractSingleOptionQuestionView<AnswerViewModel>
    {
        public SingleOptionQuestionView(
            Context context, 
            IMvxAndroidBindingContext bindingActivity, 
            QuestionViewModel source, 
            Guid questionnairePublicKey, 
            IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
            typedMode = Model as SelectebleQuestionViewModel;
        }

        private readonly SelectebleQuestionViewModel typedMode;

        protected override IEnumerable<AnswerViewModel> Answers
        {
            get { return typedMode.Answers; }
        }

        protected override bool IsAnswerSelected(AnswerViewModel answer)
        {
            return answer.Selected;
        }

        protected override string GetAnswerId(AnswerViewModel answer)
        {
            return answer.PublicKey.ToString();
        }

        protected override string GetAnswerTitle(AnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override AnswerViewModel FindAnswerInModelByRadioButtonTag(string tag)
        {
            var answerGuid = Guid.Parse(tag);
            return Answers.FirstOrDefault(
                a => a.PublicKey == answerGuid);
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(AnswerViewModel selectedAnswer)
        {
           return new AnswerSingleOptionQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow,
                selectedAnswer.Value);
        }

        protected override void AddAdditionalAttributes(RadioButton radioButton, AnswerViewModel answer)
        {
            base.AddAdditionalAttributes(radioButton, answer);
            radioButton.AttachImage(answer);
        }
    }
}