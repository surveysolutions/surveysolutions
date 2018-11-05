using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class IdentifyingAnswer
    {
        public static IdentifyingAnswer Create(Assignment assignment, IQuestionnaire questionnaire, 
            string answer,
            Identity questionId = null, 
            string variableName = null, 
            bool transformAnswers = false)
        {
            var result = new IdentifyingAnswer
            {
                Assignment = assignment
            };

            result.Answer = result.AnswerAsString = answer;

            result.TryToFillQuestionId(questionnaire, questionId, variableName);

            if (string.IsNullOrWhiteSpace(result.VariableName) || result.Identity == null)
            {
                throw new ArgumentException($"Cannot identify question from provided data: questionId: {questionId}, variable: {variableName}");
            }
            
            var questionType = questionnaire.GetQuestionType(result.Identity.Id);
            if (questionType == QuestionType.SingleOption)
            {
                if (!int.TryParse(answer, out var singleOptionAnswer))
                {
                    throw new ArgumentException($@"Incorrect answer type for SingleOption question: '{result.VariableName}' => '{answer}'", nameof(answer));
                }

                result.AnswerAsString = questionnaire.GetAnswerOptionTitle(result.Identity.Id, singleOptionAnswer);
            }

            if (questionType == QuestionType.DateTime)
            {
                if (DateTime.TryParse(answer, out DateTime parsedResult))
                {
                    if (questionnaire.IsTimestampQuestion(questionId.Id))
                    {
                        result.AnswerAsString = parsedResult.ToString(DateTimeFormat.DateWithTimeFormat);
                    }
                    else
                    {
                        result.AnswerAsString = parsedResult.ToString(DateTimeFormat.DateFormat);
                    }
                }
            }

            return result;
        }

        private void TryToFillQuestionId(IQuestionnaire questionnaire, Identity questionId, string variableName)
        {
            this.Identity = questionId;
            this.VariableName = variableName;

            if (!string.IsNullOrWhiteSpace(variableName) && questionId == null)
            {
                var qId = questionnaire.GetQuestionIdByVariable(variableName);

                if (qId != null)
                {
                    this.Identity = Identity.Create(qId.Value, null);
                }
            }

            if (string.IsNullOrWhiteSpace(variableName) && questionId != null)
            {
                this.VariableName = questionnaire.GetQuestionVariableName(questionId.Id);
            }
        }

        public virtual Identity Identity { get; protected set; }

        public virtual string Answer { get; set; }

        public virtual string AnswerAsString { get; protected set; }

        public string VariableName { get; set; }

        public virtual Assignment Assignment { get; protected set; }
    }
}
