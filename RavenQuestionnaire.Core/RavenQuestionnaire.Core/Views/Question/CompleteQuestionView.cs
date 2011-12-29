using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
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

        public CompleteAnswerView[] Answers { get; private set; }

        protected QuestionView Question { get; set; }

        public string QuestionnaireId
        {
            get { return Question.QuestionnaireId; }
        }

        public bool Enabled { get; set; }

        public CompleteQuestionView()
        {
        }

        public CompleteQuestionView(RavenQuestionnaire.Core.Entities.SubEntities.Question templateQuestion, QuestionnaireDocument questionnaire)
        {
            this.Question = new QuestionView(questionnaire, templateQuestion);
            this.Answers = templateQuestion.Answers.Select(a => new CompleteAnswerView(a, templateQuestion.PublicKey, false)).ToArray();
        }

    }
}
