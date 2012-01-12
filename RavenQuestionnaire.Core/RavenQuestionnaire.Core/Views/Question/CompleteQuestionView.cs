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
            get { return 0; }
        }

        public Guid PublicKey { get; private set; }

        public string QuestionText { get; private set; }
    
        public QuestionType QuestionType
        { get; private set; }

        public CompleteAnswerView[] Answers { get; private set; }

     //   protected QuestionView Question { get; set; }

        public string QuestionnaireId { get; private set; }

        public bool Enabled { get; set; }

        public CompleteQuestionView()
        {
        }

        public CompleteQuestionView(RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion templateQuestion, string questionnaireId)
        {
            this.PublicKey = templateQuestion.PublicKey;
            this.QuestionText = templateQuestion.QuestionText;
            this.QuestionType = templateQuestion.QuestionType;
            this.QuestionnaireId = questionnaireId;
            this.Enabled = templateQuestion.Enabled;
            this.Answers = templateQuestion.Answers.Select(a => new CompleteAnswerView(a, templateQuestion.PublicKey)).ToArray();
        }

    }
}
