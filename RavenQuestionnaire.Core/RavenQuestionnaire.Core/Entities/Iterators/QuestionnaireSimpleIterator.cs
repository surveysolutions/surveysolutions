using System;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireSimpleIterator : Iterator<CompleteQuestion, Guid?>
    {
        public QuestionnaireSimpleIterator(CompleteQuestionnaire completeQuestionnaire, IExpressionExecutor<CompleteQuestionnaire, bool> validator)
        {
            this._completeQuestionnaire = completeQuestionnaire;
            this.expressionValidator = validator;
            if (this._completeQuestionnaire.GetAllQuestions().Count == 0)
                throw new ArgumentException("Questionnaires question list is empty");
        }

        private IExpressionExecutor<CompleteQuestionnaire, bool> expressionValidator;
        protected CompleteQuestionnaire _completeQuestionnaire;

        public CompleteQuestion First
        {
            get
            {
                return this._completeQuestionnaire.GetAllQuestions()[0];
            }
        }
        public CompleteQuestion Last
        {
            get
            {
                int lastIndex = this._completeQuestionnaire.GetAllQuestions().Count - 1;
                CompleteQuestion possibleQuestion = this._completeQuestionnaire.GetAllQuestions()[lastIndex];
                while (!expressionValidator.Execute(this._completeQuestionnaire, possibleQuestion.ConditionExpression))
                {
                    if (lastIndex == 0)
                        return null;
                    lastIndex--;
                    possibleQuestion = this._completeQuestionnaire.GetAllQuestions()[lastIndex];
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
                CompleteQuestion possibleQuestion = this._completeQuestionnaire.GetAllQuestions()[++this.current];
                if (expressionValidator.Execute(this._completeQuestionnaire, possibleQuestion.ConditionExpression))
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
                CompleteQuestion possibleQuestion = this._completeQuestionnaire.GetAllQuestions()[--this.current];
                if (expressionValidator.Execute(this._completeQuestionnaire, possibleQuestion.ConditionExpression))
                {
                    return possibleQuestion;
                }
                return Previous;
            }
        }

        public bool IsDone
        {
            get { return this.current >= this._completeQuestionnaire.GetAllQuestions().Count - 1; }
        }

        public CompleteQuestion CurrentItem
        {
            get { return this._completeQuestionnaire.GetAllQuestions()[current]; }
        }

        public CompleteQuestion GetNextAfter(Guid? questionkey)
        {
            if (!questionkey.HasValue)
                return First;

            var question =
                this._completeQuestionnaire.GetAllQuestions().FirstOrDefault(q => q.PublicKey.Equals(questionkey.Value));
            current = this._completeQuestionnaire.GetAllQuestions().IndexOf(question);
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
                this._completeQuestionnaire.GetAllQuestions().FirstOrDefault(q => q.PublicKey.Equals(questionkey));
            current = this._completeQuestionnaire.GetAllQuestions().IndexOf(question);
            if (question != null)
            {
                return Previous;
            }
            return null;
        }

        private int current = 0;
    }
}
