using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireSimpleIterator : Iterator<Question>
    {
        public QuestionnaireSimpleIterator(CompleteQuestionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
            if (this.questionnaire.GetAllQuestions().Count == 0)
                throw new ArgumentException("Questionnaires question list is empty");
        }

        protected CompleteQuestionnaire questionnaire;

        public Question First
        {
            get
            {
                return this.questionnaire.GetAllQuestions()[0];
            }
        }

        public Question Next
        {
            get
            {
                if (IsDone)
                    return null;
                Question possibleQuestion = this.questionnaire.GetAllQuestions()[++this.current];
                if (possibleQuestion.EvaluateCondition(this.questionnaire.GetAllAnswers()))
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
                if (possibleQuestion.EvaluateCondition(this.questionnaire.GetAllAnswers()))
                {
                    return possibleQuestion;
                }
                return Previous;
            }
        }

        public bool IsDone{
            get { return this.current >= this.questionnaire.GetAllQuestions().Count - 1; }
        }

        public Question CurrentItem
        {
            get { return this.questionnaire.GetAllQuestions()[current]; }
        }
        private int current = 0;
    }
}
