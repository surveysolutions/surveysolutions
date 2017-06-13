using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class IdentifyingAnswer
    {
        public static IdentifyingAnswer Create(Assignment assignment, IQuestionnaire questionnaire, string answer, Identity questionId = null, string variableName = null)
        {
            var result = new IdentifyingAnswer();

            result.Assignment = assignment; 
            result.Answer = result.AnswerAsString = answer;

            result.TryToFillQuestionId(questionnaire, questionId, variableName);

            if (string.IsNullOrWhiteSpace(result.VariableName) || result.Identity == null)
            {
                throw new ArgumentException($"Cannot identify question from provided data: questionId: {questionId}, variable: {variableName}");
            }

            var questionType = questionnaire.GetQuestionType(result.Identity.Id);
            if (questionType == QuestionType.SingleOption)
            {
                int singleOptionAnswer;

                if (!int.TryParse(answer, out singleOptionAnswer))
                {
                    throw new ArgumentException($@"Incorrect answer type for SingleOption question: '{result.VariableName}' => '{answer}'", nameof(answer));
                }

                result.AnswerAsString = questionnaire.GetAnswerOptionTitle(result.Identity.Id, singleOptionAnswer);
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
        
        public virtual string Answer { get; protected set; }

        public virtual string AnswerAsString { get; protected set; }

        public string VariableName { get; set; }

        public virtual Assignment Assignment { get; protected set; }
    }
}