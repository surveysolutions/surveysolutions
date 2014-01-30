using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class TextListQuestionViewModel : QuestionViewModel
    {
        public TextListQuestionViewModel(InterviewItemId publicKey, string text, QuestionType questionType,
            bool enabled, string instructions, string comments, bool valid, bool mandatory,
            string validationMessage, string variable, IEnumerable<string> substitutionReferences,
            IEnumerable<TextListAnswerViewModel> answers, int? maxAnswerCount) 
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, answers, 
                   validationMessage, variable, substitutionReferences)
        {
            this.ListAnswers = answers;
            this.MaxAnswerCount = maxAnswerCount;
        }

        public IEnumerable<TextListAnswerViewModel> ListAnswers { get; private set; }

        public int? MaxAnswerCount { get; private set; }

        public override IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            List<TextListAnswerViewModel> newAnswers = new List<TextListAnswerViewModel>();
            foreach (var textListAnswerViewModel in this.ListAnswers)
            {
                newAnswers.Add(textListAnswerViewModel.Clone() as TextListAnswerViewModel);
            }

            return new TextListQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                                                   this.SourceText, this.QuestionType, 
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                                                   this.Mandatory,
                                                   this.ValidationMessage, this.Variable, this.SubstitutionReferences,
                                                   newAnswers,
                                                   this.MaxAnswerCount);
        }

        public override void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            var typedAnswers = answer as Tuple<decimal, string>[];

            if (typedAnswers == null)
            {
                return;
            }

            this.ListAnswers = typedAnswers.Select(item => new TextListAnswerViewModel(item.Item1.ToString(), item.Item2)).ToArray();
 
            base.SetAnswer(answer);
        }

        public override string AnswerString
        {
            get
            {
                var selectedAnswers = this.ListAnswers.Select(answer => answer.Answer).ToList();
                return string.Join(", ", selectedAnswers);
            }
        }
    }
}