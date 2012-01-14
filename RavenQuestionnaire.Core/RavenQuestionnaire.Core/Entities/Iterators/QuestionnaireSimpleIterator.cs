using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireSimpleIterator : Iterator<CompleteQuestion, Guid?>
    {
        public QuestionnaireSimpleIterator(CompleteQuestionnaire questionnaire, IExpressionExecutor<CompleteQuestionnaire, bool> validator)
        {
            this.questionnaire = questionnaire;
            this.expresstionValidator = validator;
            if (this.questionnaire.GetAllQuestions().Count == 0)
                throw new ArgumentException("Questionnaires question list is empty");
        }

        private IExpressionExecutor<CompleteQuestionnaire, bool> expresstionValidator;
        protected CompleteQuestionnaire questionnaire;

        public CompleteQuestion First
        {
            get
            {
                return this.questionnaire.GetAllQuestions()[0];
            }
        }
        public CompleteQuestion Last
        {
            get
            {
                int lastIndex = this.questionnaire.GetAllQuestions().Count - 1;
                CompleteQuestion possibleQuestion = this.questionnaire.GetAllQuestions()[lastIndex];
                while (!expresstionValidator.Execute(this.questionnaire, possibleQuestion.ConditionExpression))
                {
                    if (lastIndex == 0)
                        return null;
                    lastIndex--;
                    possibleQuestion = this.questionnaire.GetAllQuestions()[lastIndex];
                }
                return possibleQuestion;
            }
        }
        public CompleteQuestion Next
        {
            get
            {
                if (IsDone)
                    return null;
                CompleteQuestion possibleQuestion = this.questionnaire.GetAllQuestions()[++this.current];
                if (expresstionValidator.Execute(this.questionnaire, possibleQuestion.ConditionExpression))
                {
                    return possibleQuestion;
                }
                return Next;
            }
        }

        public CompleteQuestion Previous
        {
            get
            {
                if (CurrentItem == First)
                    return null;
                CompleteQuestion possibleQuestion = this.questionnaire.GetAllQuestions()[--this.current];
                if (expresstionValidator.Execute(this.questionnaire, possibleQuestion.ConditionExpression))
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

        public CompleteQuestion CurrentItem
        {
            get { return this.questionnaire.GetAllQuestions()[current]; }
        }

        public CompleteQuestion GetNextAfter(Guid? questionkey)
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
        public CompleteQuestion GetPreviousBefoure(Guid? questionkey)
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
