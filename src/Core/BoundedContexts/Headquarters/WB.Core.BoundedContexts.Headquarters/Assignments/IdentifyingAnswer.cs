using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class IdentifyingAnswer
    {
        protected IdentifyingAnswer()
        {
        }

        public IdentifyingAnswer(Assignment assignment, IQuestionnaire questionnaire, string answer, Guid questionId)
        {
            this.Assignment = assignment;
            this.Answer = this.AnswerAsString = answer;

            this.QuestionId = questionId;
            var questionType = questionnaire.GetQuestionType(questionId);
            if (questionType == QuestionType.SingleOption)
            {
                this.AnswerAsString = questionnaire.GetAnswerOptionTitle(questionId, int.Parse(answer));
            }
        }

        public virtual Guid QuestionId { get; protected set; }

        public virtual string Answer { get; protected set; }

        public virtual string AnswerAsString { get; protected set; }

        public string VariableName { get; set; }

        public virtual  Assignment Assignment { get; protected set; }
    }
}