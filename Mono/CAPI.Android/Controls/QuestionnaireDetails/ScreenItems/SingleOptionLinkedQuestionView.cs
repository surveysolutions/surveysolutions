using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class SingleOptionLinkedQuestionView : AbstractSingleOptionQuestionView<LinkedAnswerViewModel>
    {
        private const char Separator = ',';

        public SingleOptionLinkedQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel model,
            Guid questionnairePublicKey,
            IAnswerOnQuestionCommandService commandService
            )
            : base(context, bindingActivity, model, questionnairePublicKey, commandService)
        {
        }

        protected LinkedQuestionViewModel TypedMode {
            get { return Model as LinkedQuestionViewModel; }
        }

        protected override IEnumerable<LinkedAnswerViewModel> Answers
        {
            get { return TypedMode.Answers; }
        }

        protected override string GetAnswerId(LinkedAnswerViewModel answer)
        {
            return string.Join(Separator.ToString(), answer.PropagationVector);
        }

        protected override string GetAnswerTitle(LinkedAnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override LinkedAnswerViewModel FindAnswerInModelByRadioButtonTag(string tag)
        {
            var vector = tag.Split(Separator).Select(int.Parse).ToArray();
            return Answers.FirstOrDefault(
              a => LinkedQuestionViewModel.IsVectorsEqual(a.PropagationVector, vector));
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(LinkedAnswerViewModel selectedAnswer)
        {
            return new AnswerSingleOptionLinkedQuestionCommand(this.QuestionnairePublicKey,
                 CapiApplication.Membership.CurrentUser.Id,
                 Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow,
                 selectedAnswer.PropagationVector);
        }

        protected override bool IsAnswerSelected(LinkedAnswerViewModel answer)
        {
            return TypedMode.SelectedAnswers.Any(a => LinkedQuestionViewModel.IsVectorsEqual(a, answer.PropagationVector));
        }
    }
}