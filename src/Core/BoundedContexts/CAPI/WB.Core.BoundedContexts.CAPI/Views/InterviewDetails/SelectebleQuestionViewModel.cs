using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{
    public class SelectebleQuestionViewModel : QuestionViewModel
    {
        public SelectebleQuestionViewModel(
            InterviewItemId publicKey,
            string text,
            QuestionType questionType,
            IEnumerable<AnswerViewModel> answers,
            bool enabled,
            string instructions,
            string comments,
            bool valid,
            bool mandatory,
            object answerObject,
            string validationMessage,
            string variable,
            IEnumerable<string> substitutionReferences,
            bool? areAnswersOrdered,
            int? maxAllowedAnswers)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory,
                   answerObject, validationMessage, variable, substitutionReferences)
        {
            this.Answers = answers;
            this.MaxAllowedAnswers = maxAllowedAnswers;
            this.AreAnswersOrdered = areAnswersOrdered;
        }

        public IEnumerable<AnswerViewModel> Answers { get; private set; }
        public int? MaxAllowedAnswers { get; private set; }
        public bool? AreAnswersOrdered { get; private set; }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers = this.Answers.Where(a => a.Selected).Select(answer => answer.Title).ToList();
                return string.Join(", ", selectedAnswers);
            }
        }

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            IList<AnswerViewModel> newAnswers = new List<AnswerViewModel>();
            foreach (AnswerViewModel answerViewModel in this.Answers)
            {
                newAnswers.Add(answerViewModel.Clone() as AnswerViewModel);
            }
            return new SelectebleQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                this.SourceText, this.QuestionType, newAnswers,
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.AnswerObject, this.ValidationMessage, this.Variable, this.SubstitutionReferences,
                this.AreAnswersOrdered, this.MaxAllowedAnswers);
        }

        public override void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            var typedAnswers = this.CastAnswerToSingleDimensionalArray(answer) ?? this.CastAnswerToDecimal(answer);

            if (typedAnswers == null)
            {
                return;
            }

            foreach (var item in this.Answers)
            {
                item.Selected = typedAnswers.Contains(item.Value);
                item.AnswerOrder = Array.IndexOf(typedAnswers, item.Value) + 1;
            }

            base.SetAnswer(answer);

            this.RemoveStatusAnsweredIfMultiOptionHasNoSelectedOptions();
        }

        public override void RemoveAnswer()
        {
            foreach (var item in this.Answers)
            {
                item.Selected = false;
            }

            base.RemoveAnswer();

            this.RemoveStatusAnsweredIfMultiOptionHasNoSelectedOptions();
        }

        private void RemoveStatusAnsweredIfMultiOptionHasNoSelectedOptions()
        {
            if (this.QuestionType == QuestionType.MultyOption && this.Status.HasFlag(QuestionStatus.Answered) && !this.Answers.Any(a => a.Selected))
            {
                this.Status &= ~QuestionStatus.Answered;
                this.RaisePropertyChanged("Status");
            }
        }

        private decimal[] CastAnswerToSingleDimensionalArray(object answer)
        {
            var decimalCast = answer as IEnumerable<decimal>;
            if (decimalCast != null)
                return decimalCast.ToArray();

            var objectCast = answer as IEnumerable<object>;
            if (objectCast != null)
                return objectCast.Select(this.ConvertObjectToAnswer).Where(a => a.HasValue).Select(a => a.Value).ToArray();

            var jArrayCast = this.GetValueFromJArray<decimal>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private decimal? ConvertObjectToAnswer(object answer)
        {
            if (answer == null)
                return null;
            decimal value;
            if (decimal.TryParse(answer.ToString(), out value))
                return value;
            return null;
        }

        private decimal[] CastAnswerToDecimal(object answer)
        {
            var decimalAnswer = this.ConvertObjectToAnswer(answer);
            if (decimalAnswer.HasValue)
                return new decimal[] { decimalAnswer.Value };
            return null;
        }
    }
}