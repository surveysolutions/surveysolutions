using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
    public class Questionnaire : IEntity<QuestionnaireDocument>
    {
        private QuestionnaireDocument innerDocument;

        public string QuestionnaireId { get { return innerDocument.Id; } }

        public Questionnaire(string title)
        {
            innerDocument = new QuestionnaireDocument() {Title = title};
        }
        public Questionnaire(QuestionnaireDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }
        public void UpdateText(string text)
        {
            innerDocument.Title = text;
            innerDocument.LastEntryDate = DateTime.Now;
        }
        public void ClearQuestions()
        {
            innerDocument.Questions.Clear();
        }

        public Question AddQuestion(string text, QuestionType type)
        {
            Question result = new Question()
                                  {QuestionText = text, QuestionType = type, QuestionnaireId = this.QuestionnaireId};
            innerDocument.Questions.Add(result);
            return result;
        }

        QuestionnaireDocument IEntity<QuestionnaireDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }
        public bool RemoveQuestion(Guid publicKey)
        {
            return innerDocument.Questions.RemoveAll(q => q.PublicKey.Equals(publicKey)) > 0;
        }
        public void UpdateQuestion(Guid publicKey, string text, QuestionType type,string condition, IEnumerable<Answer> answers)
        {
            var question = innerDocument.Questions.Where(q => q.PublicKey.Equals(publicKey)).FirstOrDefault();
            if (question == null)
                return;
            question.QuestionText = text;
            question.QuestionType = type;
            question.UpdateAnswerList(answers);
            question.SetConditionExpression(condition);
            //     return innerDocument.Questions.RemoveAll(q => q.PublicKey.Equals(publicKey)) > 0;
        }
    }
}
