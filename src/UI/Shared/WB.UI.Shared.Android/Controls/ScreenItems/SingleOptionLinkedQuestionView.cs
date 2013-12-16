using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems{

    public class SingleOptionLinkedQuestionView : AbstractSingleOptionQuestionView<LinkedAnswerViewModel>
    {
        private const char Separator = ',';

        public SingleOptionLinkedQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel model,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, model, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.Model.PropertyChanged += this.Model_PropertyChanged;
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool answerOptionsChanged = e.PropertyName == "AnswerOptions";
            if (!answerOptionsChanged)
                return;

            this.FillRadioButtonGroupWithAnswers();
        }

        protected LinkedQuestionViewModel TypedMode {
            get { return this.Model as LinkedQuestionViewModel; }
        }

        protected override IEnumerable<LinkedAnswerViewModel> Answers
        {
            get { return this.TypedMode.AnswerOptions; }
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
            var vector = tag.Split(Separator).Select(decimal.Parse).ToArray();
            return this.Answers.FirstOrDefault(
              a => LinkedQuestionViewModel.IsVectorsEqual(a.PropagationVector, vector));
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(LinkedAnswerViewModel selectedAnswer)
        {
            return new AnswerSingleOptionLinkedQuestionCommand(this.QuestionnairePublicKey,
                 this.Membership.CurrentUser.Id,
                 this.Model.PublicKey.Id, this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow,
                 selectedAnswer.PropagationVector);
        }

        protected override bool IsAnswerSelected(LinkedAnswerViewModel answer)
        {
            return this.TypedMode.SelectedAnswers.Any(a => LinkedQuestionViewModel.IsVectorsEqual(a, answer.PropagationVector));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Model != null)
                {
                    this.Model.PropertyChanged -= this.Model_PropertyChanged;
                }
            }

            base.Dispose(disposing);
        }
    }
}