using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyQuestionView : AbstractMultyQuestionView<AnswerViewModel>
    {
        public MultyQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
            Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService) {}

        protected SelectebleQuestionViewModel TypedMode
        {
            get { return Model as SelectebleQuestionViewModel; }
        }

        protected override IEnumerable<AnswerViewModel> Answers
        {
            get { return TypedMode.Answers; }
        }

        protected override string GetAnswerId(AnswerViewModel answer)
        {
            return answer.PublicKey.ToString();
        }

        protected override string GetAnswerTitle(AnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override AnswerViewModel FindAnswerInModelByCheckBoxTag(string tag)
        {
            var answerGuid = Guid.Parse(tag);
            return Answers.FirstOrDefault(
                a => a.PublicKey == answerGuid);
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(AnswerViewModel selectedAnswer, bool isChecked)
        {
            var answered = Answers.Where(a => a.Selected).Select(a => a.Value).ToList();

            if (isChecked)
                answered.Add(selectedAnswer.Value);
            else
            {
                answered.Remove(selectedAnswer.Value);
            }

            return new AnswerMultipleOptionsQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow, answered.ToArray());
        }

        protected override bool IsAnswerSelected(AnswerViewModel answer)
        {
            return answer.Selected;
        }

        protected override void AddAdditionalAttributes(CheckBox checkBox, AnswerViewModel answer)
        {
            base.AddAdditionalAttributes(checkBox, answer);
            checkBox.AttachImage(answer);
        }
    }
}