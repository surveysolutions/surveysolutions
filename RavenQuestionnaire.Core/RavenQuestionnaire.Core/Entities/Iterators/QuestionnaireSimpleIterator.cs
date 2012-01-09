using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireSimpleIterator : Iterator<Question, Guid?>
    {
        public QuestionnaireSimpleIterator(CompleteQuestionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
            if (this.questionnaire.GetAllQuestions().Count == 0)
                throw new ArgumentException("Questionnaires question list is empty");
        }
        private CompleteQuestionnaireConditionExecutor expresstionValidator;

        protected CompleteQuestionnaireConditionExecutor ExpresstionValidator
        {
            get
            {
                if (expresstionValidator == null)
                    expresstionValidator = new CompleteQuestionnaireConditionExecutor(this.questionnaire.GetInnerDocument());
                return expresstionValidator;
            }
        }
        protected CompleteQuestionnaire questionnaire;

        public Question First
        {
            get
            {
                return this.questionnaire.GetAllQuestions()[0];
            }
        }
        public Question Last
        {
            get
            {
                int lastIndex = this.questionnaire.GetAllQuestions().Count - 1;
                Question possibleQuestion = this.questionnaire.GetAllQuestions()[lastIndex];
                while (!ExpresstionValidator.Execute(possibleQuestion.ConditionExpression))
                {
                    if (lastIndex == 0)
                        return null;
                    lastIndex--;
                    possibleQuestion = this.questionnaire.GetAllQuestions()[lastIndex];
                }
                return possibleQuestion;
            }
        }
        public Question Next
        {
            get
            {
                if (IsDone)
                    return null;
                Question possibleQuestion = this.questionnaire.GetAllQuestions()[++this.current];
                if (ExpresstionValidator.Execute(possibleQuestion.ConditionExpression))
                {
                    return possibleQuestion;
                }
                return Next;
            }
        }

        public Question Previous
        {
            get
            {
                if (CurrentItem == First)
                    return null;
                Question possibleQuestion = this.questionnaire.GetAllQuestions()[--this.current];
                if (ExpresstionValidator.Execute(possibleQuestion.ConditionExpression))
                {
                    return possibleQuestion;
                }
                return Previous;
            }
        }

        public bool IsDone
        {
            get { return this.current >= this.questionnaire.GetAllQuestions().Count - 1; }
        }

        public Question CurrentItem
        {
            get { return this.questionnaire.GetAllQuestions()[current]; }
        }

        public Question GetNextAfter(Guid? questionkey)
        {
            if (!questionkey.HasValue)
                return First;

            var question =
                this.questionnaire.GetAllQuestions().FirstOrDefault(q => q.PublicKey.Equals(questionkey.Value));
            current = this.questionnaire.GetAllQuestions().IndexOf(question);
            if (question != null)
            {
                return Next;
            }
            return null;
        }
        public Question GetPreviousBefoure(Guid? questionkey)
        {
            if (!questionkey.HasValue)
                return Last;
            var question =
                this.questionnaire.GetAllQuestions().FirstOrDefault(q => q.PublicKey.Equals(questionkey));
            current = this.questionnaire.GetAllQuestions().IndexOf(question);
            if (question != null)
            {
                return Previous;
            }
            return null;
        }

        private int current = 0;
    }
}
