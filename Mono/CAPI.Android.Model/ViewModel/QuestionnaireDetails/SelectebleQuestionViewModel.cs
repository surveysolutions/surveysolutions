using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
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
            bool capital,
            object answerObject,
            string validationMessage)
            : base(
                publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, capital, answerObject, validationMessage)
        {
            Answers = answers;
        }

        public IEnumerable<AnswerViewModel> Answers { get; private set; }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers = Answers.Where(a => a.Selected).Select(answer => answer.Title).ToList();
                return string.Join(", ", selectedAnswers);
            }
        }

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            IList<AnswerViewModel> newAnswers = new List<AnswerViewModel>();
            foreach (AnswerViewModel answerViewModel in Answers)
            {
                newAnswers.Add(answerViewModel.Clone() as AnswerViewModel);
            }
            return new SelectebleQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                this.Text, this.QuestionType, newAnswers,
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.Capital, this.AnswerObject, this.ValidationMessage);
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
            var simpleCast = answer as decimal[];
            if (simpleCast != null)
                return simpleCast;
            var jArrayCast = this.GetValueFromJArray<decimal>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private decimal[] CastAnswerToDecimal(object answer)
        {
            decimal decimalAnswer;
            if (!decimal.TryParse(answer.ToString(), out decimalAnswer))
                return null;
            return new decimal[] { decimalAnswer };
        }
    }
}