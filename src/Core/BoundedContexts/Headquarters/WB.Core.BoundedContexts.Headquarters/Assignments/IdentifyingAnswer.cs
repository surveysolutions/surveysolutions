using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class IdentifyingAnswer
    {
        public static IdentifyingAnswer Create(Assignment assignment, IQuestionnaire questionnaire, string answer, Guid? questionId = null, string variableName = null)
        {
            var result = new IdentifyingAnswer();

            result.Assignment = assignment;
            result.Answer = result.AnswerAsString = answer;

            result.TryToFillQuestionId(questionnaire, questionId, variableName);

            if (string.IsNullOrWhiteSpace(result.VariableName) || result.QuestionId == Guid.Empty)
            {
                throw new ArgumentException($"Cannot identify question from provided data: questionId: {questionId}, variable: {variableName}");
            }

            var questionType = questionnaire.GetQuestionType(result.QuestionId);
            if (questionType == QuestionType.SingleOption)
            {
                int singleOptionAnswer;

                if (!int.TryParse(answer, out singleOptionAnswer))
                {
                    throw new ArgumentException($@"Incorrect answer type for SingleOption question: '{result.VariableName}' => '{answer}'", nameof(answer));
                }

                result.AnswerAsString = questionnaire.GetAnswerOptionTitle(result.QuestionId, singleOptionAnswer);
            }

            return result;
        }

        private void TryToFillQuestionId(IQuestionnaire questionnaire, Guid? questionId, string variableName)
        {
            this.QuestionId = questionId ?? Guid.Empty;
            this.VariableName = variableName;

            if (!string.IsNullOrWhiteSpace(variableName) && questionId == null)
            {
                questionId = questionnaire.GetQuestionIdByVariable(variableName);

                if (questionId.HasValue)
                {
                    this.QuestionId = questionId.Value;
                }
            }

            if (string.IsNullOrWhiteSpace(variableName) && questionId.HasValue)
            {
                this.VariableName = questionnaire.GetQuestionVariableName(questionId.Value);
            }
        }

        public virtual Guid QuestionId { get; protected set; }

        public virtual string Answer { get; protected set; }

        public virtual string AnswerAsString { get; protected set; }

        public string VariableName { get; set; }

        public virtual  Assignment Assignment { get; protected set; }
    }
}