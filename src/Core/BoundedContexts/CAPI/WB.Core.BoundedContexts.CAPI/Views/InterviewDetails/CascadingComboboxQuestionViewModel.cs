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
            InterviewItemId publicKey, 
            ValueVector<Guid> questionRosterScope,
            string text,
            Func<decimal[], IEnumerable<AnswerViewModel>> getAnswerOptions,
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

        private readonly Func<decimal[], IEnumerable<AnswerViewModel>> getAnswerOptions;

        internal IEnumerable<AnswerViewModel> filteredAnswers;

        public IEnumerable<AnswerViewModel> AnswerOptions
        {
            get
            {
                if (this.filteredAnswers == null)
                {
                    this.UpdateOptionsList();
                }
                return this.filteredAnswers;
            }
        }

        public void HandleAnswerListChange()
        {
            UpdateOptionsList();
            this.RaisePropertyChanged("AnswerOptions");
        }

        public override string AnswerString
        {
            get
            {
                return string.Join(", ", this.filteredAnswers.Where(a => a.Selected).Select(answer => answer.Title));
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
            var typedAnswers = QuestionUtils.ExtractSelectedOptions(answer ?? "");

            if (typedAnswers == null)
            {
                return;
            }

            var selectedAnswer = typedAnswers[0];

            foreach (var item in this.filteredAnswers ?? AnswerOptions)
            {
                item.Selected = selectedAnswer == item.Value;
            }

            base.SetAnswer(answer);
        }

        public override void RemoveAnswer()
        {
            foreach (var item in this.filteredAnswers ?? AnswerOptions)
            {
                item.Selected = false;
            }

            base.RemoveAnswer();

            this.Status &= ~QuestionStatus.Answered;
            this.RaisePropertyChanged("Status");
            this.RaisePropertyChanged("AnswerRemoved");
        }

        private void UpdateOptionsList()
        {
            this.filteredAnswers = this.getAnswerOptions(this.PublicKey.InterviewItemPropagationVector);
        }
    }
}