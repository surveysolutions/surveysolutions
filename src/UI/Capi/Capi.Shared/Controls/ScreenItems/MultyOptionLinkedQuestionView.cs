using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public class MultyOptionLinkedQuestionView: AbstractMultyQuestionView<LinkedAnswerViewModel>
    {
        private const char Separator = ',';

        public MultyOptionLinkedQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel model,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, model, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.Model.PropertyChanged += this.ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            bool answerOptionsChanged = propertyChangedEventArgs.PropertyName == "AnswerOptions";
            if (!answerOptionsChanged)
                return;

            this.CreateAnswersByOptions();
        }

        protected LinkedQuestionViewModel TypedModel {
            get { return this.Model as LinkedQuestionViewModel; }
        }

        protected override IEnumerable<LinkedAnswerViewModel> Answers
        {
            get { return this.TypedModel.AnswerOptions; }
        }

        protected override int? MaxAllowedAnswers
        {
            get { return this.TypedModel.MaxAllowedAnswers; }
        }

        protected override bool? AreAnswersOrdered
        {
            get { return this.TypedModel.AreAnswersOrdered; }
        }

        protected override string GetAnswerId(LinkedAnswerViewModel answer)
        {
            return string.Join(Separator.ToString(), answer.PropagationVector);
        }

        protected override string GetAnswerTitle(LinkedAnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override LinkedAnswerViewModel FindAnswerInModelByCheckBoxTag(string tag)
        {
            var vector = tag.Split(Separator).Select(int.Parse).ToArray();
            return this.Answers.FirstOrDefault(
              a => LinkedQuestionViewModel.IsVectorsEqual(a.PropagationVector, vector));
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(LinkedAnswerViewModel[] selectedAnswers)
        {
            var elements = selectedAnswers.Select(a => new Tuple<int[],int> (a.PropagationVector, this.GetAnswerOrder(a))).ToList();

            var answered = elements.OrderBy(tuple => tuple.Item2).Where(w => w.Item2 > 0).Select(a => a.Item1).
                Union(elements.Where(w => w.Item2 == 0).Select(a => a.Item1));
            
            return new AnswerMultipleOptionsLinkedQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow,
                answered.ToArray());
        }

        protected override bool IsAnswerSelected(LinkedAnswerViewModel answer)
        {
            return this.TypedModel.SelectedAnswers.Any(a => LinkedQuestionViewModel.IsVectorsEqual(a, answer.PropagationVector));
        }

        protected override int GetAnswerOrder(LinkedAnswerViewModel answer)
        {
            return Array.FindIndex(this.TypedModel.SelectedAnswers, a => LinkedQuestionViewModel.IsVectorsEqual(a, answer.PropagationVector)) + 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Model != null)
                {
                    this.Model.PropertyChanged -= this.ModelOnPropertyChanged;
                }
            }

            base.Dispose(disposing);
        }
    }
}