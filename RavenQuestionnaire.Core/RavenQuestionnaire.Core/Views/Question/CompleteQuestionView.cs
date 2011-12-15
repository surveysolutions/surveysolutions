using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class CompleteQuestionView 
    {
        public int Index
        {
            get { return Question.Index; }
        }

        public Guid PublicKey
        {
            get { return Question.PublicKey; }
        }

        public string QuestionText { get { return Question.QuestionText; }
        }
        public QuestionType QuestionType
        {
            get { return Question.QuestionType; }
        }

        public CompleteAnswerView[] Answers
        {
            get
            {
                if (_answers == null)
                {
                    _answers = Question.Answers.Select(a => new CompleteAnswerView(a, false)).ToArray();
                }
                return _answers;
            }
            
        }

        private CompleteAnswerView[] _answers;
        protected QuestionView Question { get; set; }

        public string QuestionnaireId
        {
            get { return Question.QuestionnaireId; }
        }
        public CompleteQuestionView()
        {
        }

        public CompleteQuestionView(QuestionView templateQuestion)
        {
            this.Question = templateQuestion;
        }

    }
}
