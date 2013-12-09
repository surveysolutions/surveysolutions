using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
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
            this.SelectedAnswers = new int[0][];
            this.AreAnswersOrdered = areAnsewrsOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
        }

        private Func<IEnumerable<LinkedAnswerViewModel>> getAnswerOptions;

        public IEnumerable<LinkedAnswerViewModel> AnswerOptions
        {
            get { return this.getAnswerOptions(); }
        }

        public int[][] SelectedAnswers { get; private set; }
        public bool? AreAnswersOrdered { get; private set; }
        public int? MaxAllowedAnswers { get; private set; }

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
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
            int[][] typedAnswers = this.CastAnswer(answer);

            if (this.IsAnswerNull(typedAnswers))
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
            this.SelectedAnswers = new int[][]{};

            base.RemoveAnswer();

            if (this.IsMultiOptionQuestionHasNoSelectedOptions)
            {
                this.RemoveAnsweredFromStatus();
            }
        }

        private int[][] CastAnswer(object answer)
        {
            if (this.IsAnswerNull(answer))
                return null;

            var selectedAnswer = this.CastAnswerToSingleDimensionalArray(answer);
            if (selectedAnswer != null)
                return new int[][] { selectedAnswer };

            int[][] typedAnswers = this.CastAnswerTo2DimensionalArray(answer);
            if (typedAnswers != null)
                return typedAnswers;
        
            return new int[0][];
        }

        private int[] CastAnswerToSingleDimensionalArray(object answer)
        {
            var intCast = answer as IEnumerable<int>;
            if (intCast != null)
                return intCast.ToArray();

            var objectCast = this.CastAnswerFormObjectToIntArray(answer);
            if (objectCast != null)
                return objectCast;
            
            var jArrayCast = this.GetValueFromJArray<int>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private int[][] CastAnswerTo2DimensionalArray(object answer)
        {
            var objectCast = this.CastAnswerFormObject2DimensionalToIntArray(answer);
            if (objectCast != null)
                return objectCast;

            var jArrayCast = this.GetValueFromJArray<int[]>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private int[][] CastAnswerFormObject2DimensionalToIntArray(object answer)
        {
            var intCast = answer as IEnumerable<int[]>;
            if (intCast != null)
                return intCast.ToArray();

            var objectCast = answer as IEnumerable<object>;
            if (objectCast == null || !objectCast.Any())
                return null;

            var result = objectCast.Select(this.CastAnswerFormObjectToIntArray).Where(i => i != null).ToArray();
            if (result.Length == 0)
                return null;
            return result;
        }

        private int[] CastAnswerFormObjectToIntArray(object answer)
        {
            var objectCast = answer as IEnumerable<object>;
            if (objectCast == null)
                return null;
            
            var result = objectCast.Select(this.ConvertObjectToAnswer).Where(a => a.HasValue).Select(a => a.Value).ToArray();
            if (result.Length == 0)
                return null;
            
            return result;
        }

        private int? ConvertObjectToAnswer(object answer)
        {
            if (answer == null)
                return null;
            int value;
            if (int.TryParse(answer.ToString(), out value))
                return value;
            return null;
        }

        private bool IsAnswerNull(object answer)
        {
            return answer == null;
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