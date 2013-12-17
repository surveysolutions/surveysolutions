using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class LinkedQuestionViewModel : QuestionViewModel
    {
        public LinkedQuestionViewModel(
            InterviewItemId publicKey, string text, QuestionType questionType, bool enabled, string instructions,
            bool valid, bool mandatory, string validationMessage, Func<IEnumerable<LinkedAnswerViewModel>> getAnswerOptions,
            string variable, IEnumerable<string> substitutionReferences, bool? areAnsewrsOrdered,int? maxAllowedAnswers)
            : base(
                publicKey, text, questionType, enabled, instructions, null, valid, mandatory, null, validationMessage, variable, substitutionReferences)
        {
            this.getAnswerOptions = getAnswerOptions;
            this.SelectedAnswers = new decimal[0][];
            this.AreAnswersOrdered = areAnsewrsOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
        }

        private Func<IEnumerable<LinkedAnswerViewModel>> getAnswerOptions;

        public IEnumerable<LinkedAnswerViewModel> AnswerOptions
        {
            get { return this.getAnswerOptions(); }
        }

        public decimal[][] SelectedAnswers { get; private set; }
        public bool? AreAnswersOrdered { get; private set; }
        public int? MaxAllowedAnswers { get; private set; }

        public override IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new LinkedQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                this.SourceText, this.QuestionType,
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.ValidationMessage, this.getAnswerOptions, this.Variable, this.SubstitutionReferences, 
                this.AreAnswersOrdered, this.MaxAllowedAnswers);
        }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers =
                    this.AnswerOptions.Where(a => this.SelectedAnswers.Any(selected => IsVectorsEqual(selected, a.PropagationVector)))
                        .Select(answer => answer.Title)
                        .ToList();
                return string.Join(", ", selectedAnswers);
            }
        }

        public override void SetAnswer(object answer)
        {
            decimal[][] typedAnswers = QuestionUtils.ExtractSelectedOptionsOfLinkedQuestion(answer);

            if (typedAnswers==null)
            {
                return;
            }

            this.SelectedAnswers = typedAnswers;

            base.SetAnswer(answer);

            if (this.IsMultiOptionQuestionHasNoSelectedOptions)
            {
                this.RemoveAnsweredFromStatus();
            }
        }

        public override void RemoveAnswer()
        {
            this.SelectedAnswers = new decimal[][] { };

            base.RemoveAnswer();

            if (this.IsMultiOptionQuestionHasNoSelectedOptions)
            {
                this.RemoveAnsweredFromStatus();
            }
        }
        
        public void HandleAnswerListChange()
        {
            this.RaisePropertyChanged("AnswerOptions");
        }

        public static bool IsVectorsEqual(decimal[] vector1, decimal[] vector2)
        {
            if (vector1.Length != vector2.Length)
                return false;
            return !vector1.Where((t, i) => t != vector2[i]).Any();
        }

        private bool IsMultiOptionQuestionHasNoSelectedOptions
        {
            get
            {
                return this.QuestionType == QuestionType.MultyOption && this.Status.HasFlag(QuestionStatus.Answered) &&
                    this.SelectedAnswers.Length == 0;
            }
        }

        private void RemoveAnsweredFromStatus()
        {
            this.Status &= ~QuestionStatus.Answered;
            this.RaisePropertyChanged("Status");
        }
    }
}