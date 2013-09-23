using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class LinkedQuestionViewModel : QuestionViewModel
    {
        public LinkedQuestionViewModel(
            InterviewItemId publicKey,
            string text,
            QuestionType questionType,
            bool enabled,
            string instructions,
            bool valid,
            bool mandatory,
            bool capital,
            string validationMessage, Func<int[], IEnumerable<LinkedAnswerViewModel>> getAnswers)
            : base(
                publicKey, text, questionType, enabled, instructions, null, valid, mandatory, capital, null, validationMessage)
        {
            this.getAnswers = getAnswers;
            this.SelectedAnswers = new int[0][];
        }

        private Func<int[], IEnumerable<LinkedAnswerViewModel>> getAnswers;

        public IEnumerable<LinkedAnswerViewModel> Answers {
            get { return getAnswers(this.PublicKey.PropagationVector); }
        }

        public int[][] SelectedAnswers { get; private set; }

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new LinkedQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                this.Text, this.QuestionType, 
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.Capital, this.ValidationMessage, getAnswers);
        }

        public override void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            var typedAnswers = answer as int[][];

            if (typedAnswers == null)
            {
                var selectedAnswer = answer as int[];
                if (selectedAnswer==null)
                    return;
                typedAnswers = new int[][] { selectedAnswer };
            }

            SelectedAnswers = typedAnswers;

            base.SetAnswer(answer);

            if (this.IsMultyOptionQuestionHasNoSelectedOptions)
            {
                Status &= ~QuestionStatus.Answered;
                RaisePropertyChanged("Status");
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
    }
}