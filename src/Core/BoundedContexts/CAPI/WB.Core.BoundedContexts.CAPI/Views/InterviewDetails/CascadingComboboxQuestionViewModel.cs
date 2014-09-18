using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class CascadingComboboxQuestionViewModel : QuestionViewModel
    {
        public CascadingComboboxQuestionViewModel(
            InterviewItemId publicKey, ValueVector<Guid> questionRosterScope,
            string text,
            Func<decimal[], ValueVector<Guid>, IEnumerable<AnswerViewModel>> getAnswerOptions,
            bool enabled,
            string instructions,
            string comments,
            bool valid,
            bool mandatory,
            object answerObject,
            string validationMessage,
            string variable,
            IEnumerable<string> substitutionReferences)
            : base(publicKey,
                questionRosterScope,
                text,
                QuestionType.SingleOption,
                enabled,
                instructions,
                comments,
                valid,
                mandatory,
                answerObject,
                validationMessage,
                variable,
                substitutionReferences)
        {
            this.getAnswerOptions = getAnswerOptions;
        }

        private Func<decimal[], ValueVector<Guid>, IEnumerable<AnswerViewModel>> getAnswerOptions;

        private IEnumerable<AnswerViewModel> filteredAnswers;

        public IEnumerable<AnswerViewModel> AnswerOptions
        {
            get
            {
                this.filteredAnswers = this.getAnswerOptions(PublicKey.InterviewItemPropagationVector, QuestionRosterScope);
                return this.filteredAnswers;

            }
        }

        public void HandleAnswerListChange()
        {
            this.RaisePropertyChanged("AnswerOptions");
        }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers = this.filteredAnswers.Where(a => a.Selected).OrderBy(x => x.AnswerOrder).Select(answer => answer.Title).ToList();
                return string.Join(", ", selectedAnswers);
            }
        }

        public override IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new CascadingComboboxQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector), this.QuestionRosterScope,
                this.SourceText, getAnswerOptions,
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.AnswerObject, this.ValidationMessage, this.Variable, this.SubstitutionReferences);
        }

        public override void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            var typedAnswers = QuestionUtils.ExtractSelectedOptions(answer);

            if (typedAnswers == null)
            {
                return;
            }

            foreach (var item in this.filteredAnswers)
            {
                item.Selected = typedAnswers.Contains(item.Value);
                item.AnswerOrder = Array.IndexOf(typedAnswers, item.Value) + 1;
            }

            base.SetAnswer(answer);
        }

        public override void RemoveAnswer()
        {
            foreach (var item in this.filteredAnswers)
            {
                item.Selected = false;
            }

            base.RemoveAnswer();

            this.RemoveAnsweredFromStatus();
        }

        private void RemoveAnsweredFromStatus()
        {
            this.Status &= ~QuestionStatus.Answered;
            this.RaisePropertyChanged("Status");
        }
    }
}