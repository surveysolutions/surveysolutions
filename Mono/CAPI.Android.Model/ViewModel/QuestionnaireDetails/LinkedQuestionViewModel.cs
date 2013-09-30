using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class LinkedQuestionViewModel : QuestionViewModel
    {
        public LinkedQuestionViewModel(
            InterviewItemId publicKey, string text, QuestionType questionType, bool enabled, string instructions, bool valid,
            bool mandatory, bool capital, string validationMessage, Func<IEnumerable<LinkedAnswerViewModel>> getAnswerOptions)
            : base(
                publicKey, text, questionType, enabled, instructions, null, valid, mandatory, capital, null, validationMessage)
        {
            this.getAnswerOptions = getAnswerOptions;
            this.SelectedAnswers = new int[0][];
        }

        private Func<IEnumerable<LinkedAnswerViewModel>> getAnswerOptions;

        public IEnumerable<LinkedAnswerViewModel> AnswerOptions
        {
            get { return this.getAnswerOptions(); }
        }

        public int[][] SelectedAnswers { get; private set; }

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new LinkedQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                this.Text, this.QuestionType, 
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.Capital, this.ValidationMessage, this.getAnswerOptions);
        }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers =
                    AnswerOptions.Where(a => SelectedAnswers.Any(selected => IsVectorsEqual(selected, a.PropagationVector)))
                        .Select(answer => answer.Title)
                        .ToList();
                return string.Join(", ", selectedAnswers);
            }
        }

        public override void SetAnswer(object answer)
        {
            int[][] typedAnswers = CastAnswer(answer);

            if (IsAnswerNull(typedAnswers))
            {
                return;
            }

            SelectedAnswers = typedAnswers;

            base.SetAnswer(answer);

            if (this.IsMultyOptionQuestionHasNoSelectedOptions)
            {
                this.RemoveAnsweredFromStatus();
            }
        }

        private int[][] CastAnswer(object answer)
        {
            if (IsAnswerNull(answer))
            {
                return null;
            }

            int[][] typedAnswers = this.CastAnswerTo2DimensionalArray(answer);

            if (typedAnswers == null)
            {
                var selectedAnswer = CastAnswerToSingleDimensionalArray(answer);

                if (selectedAnswer == null)
                    return null;

                return new int[][] { selectedAnswer };
            }
            return typedAnswers;
        }

        private int[] CastAnswerToSingleDimensionalArray(object answer)
        {
            var simpleCast = answer as int[];
            if (simpleCast != null)
                return simpleCast;
            var jArrayCast = this.GetValueFromJArray<int>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private int[][] CastAnswerTo2DimensionalArray(object answer)
        {
            var simpleCast = answer as int[][];
            if (simpleCast != null)
                return simpleCast;
            var jArrayCast = this.GetValueFromJArray<int[]>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private void RemoveAnsweredFromStatus()
        {
            this.Status &= ~QuestionStatus.Answered;
            this.RaisePropertyChanged("Status");
        }

        private bool IsAnswerNull(object answer)
        {
            return answer == null;
        }

        private T[] GetValueFromJArray<T>(object answer)
        {
            try
            {
                return ((JArray) answer).ToObject<T[]>();
            }
            catch (Exception)
            {
                return new T[0];
            }
        }

        private bool IsMultyOptionQuestionHasNoSelectedOptions
        {
            get
            {
                return this.QuestionType == QuestionType.MultyOption && this.Status.HasFlag(QuestionStatus.Answered) &&
                    this.SelectedAnswers.Length == 0;
            }
        }

        public void HandleAnswerListChange()
        {
            this.RaisePropertyChanged("AnswerOptions");
        }


        public static bool IsVectorsEqual(int[] vector1, int[] vector2)
        {
            if (vector1.Length != vector2.Length)
                return false;
            return !vector1.Where((t, i) => t != vector2[i]).Any();
        }
    }
}