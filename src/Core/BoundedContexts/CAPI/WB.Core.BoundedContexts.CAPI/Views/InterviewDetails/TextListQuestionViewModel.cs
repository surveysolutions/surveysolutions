using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class TextListQuestionViewModel : QuestionViewModel
    {
        public TextListQuestionViewModel(InterviewItemId publicKey, ValueVector<Guid> questionRosterScope, string text, QuestionType questionType,
            bool enabled, string instructions, string comments, bool valid, bool mandatory,
            string validationMessage, string variable, IEnumerable<string> substitutionReferences,
            int? maxAnswerCount, int maxAnswerCountLimit,
            string[] triggeredRosters)
            : base(publicKey, questionRosterScope, text, questionType, enabled, instructions, comments, valid, mandatory, null, 
                   validationMessage, variable, substitutionReferences, triggeredRosters)
        {
            this.ListAnswers = Enumerable.Empty<TextListAnswerViewModel>();
            this.MaxAnswerCount = maxAnswerCount;
            this.MaxAnswerCountLimit = maxAnswerCountLimit;
        }

        public IEnumerable<TextListAnswerViewModel> ListAnswers { get; private set; }

        public int? MaxAnswerCount { get; private set; }

        public int MaxAnswerCountLimit { get; private set; }

        public override IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new TextListQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector), QuestionRosterScope,
                                                   this.SourceText, this.QuestionType, 
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                                                   this.Mandatory,
                                                   this.ValidationMessage, this.Variable, this.SubstitutionReferences,
                                                   this.MaxAnswerCount, this.MaxAnswerCountLimit, this.TriggeredRosters);
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
                var jContainer = answer as JContainer;

                // Happens after interview rejection
                if (jContainer != null)
                {
                    typedAnswers = jContainer
                        .ToList()
                        .Select(x => new Tuple<decimal, string>(decimal.Parse(x["Item1"].ToString()), x["Item2"].ToString()))
                        .ToArray();
                }
                else
                {
                    return;
                }
            }

            this.ListAnswers = typedAnswers.Select(item => new TextListAnswerViewModel(item.Item1, item.Item2)).ToArray();
 
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