using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Question /*: IEntity<QuestionDocument>*/
    {

        public Question()
        {
            PublicKey = Guid.NewGuid();
            Answers= new List<Answer>();
        }
        public Question(Questionnaire owner, string text, QuestionType type): this()
        {
            //innerDocument = new QuestionDocument() { QuestionText = text, QuestionType = type, QuestionnaireId = owner.QuestionnaireId };
            QuestionnaireId = owner.QuestionnaireId;
            QuestionText = text;
            QuestionType = type;
        }
        public Guid PublicKey { get; set; }
        public string QuestionText{ get; set; }
        public QuestionType QuestionType { get; set; }
        public string QuestionnaireId { get; set; }
        public List<Answer> Answers { get; set; }

   /*     private QuestionDocument innerDocument;

        public string QuestionId { get { return innerDocument.Id; } }

        public Question(Questionnaire owner, string text, QuestionType type)
        {
            innerDocument = new QuestionDocument() {QuestionText = text, QuestionType = type, QuestionnaireId = owner.QuestionnaireId};
        }

        public void UpdateQuestion(string text, QuestionType type)
        {
            innerDocument.QuestionText = text;
            innerDocument.QuestionType = type;
        }

        public Question(QuestionDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }*/

        public void ClearAnswers()
        {
            Answers.Clear();
        }

        public void AddAnswer(Answer answer)
        {
           // answer.QuestionId = this.QuestionId;
            if(Answers.Where(a => a.PublicKey.Equals(answer.PublicKey)).Count()>0)
                throw  new DuplicateNameException("answer with current publick key already exist");
            Answers.Add(answer);
        }
        public void UpdateAnswerList(IEnumerable<Answer> answers)
        {
            ClearAnswers();
            foreach (var answer in answers)
            {
                AddAnswer(answer);
            }
        }
    }
}
