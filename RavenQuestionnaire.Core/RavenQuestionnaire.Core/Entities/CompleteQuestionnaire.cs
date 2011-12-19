using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>
    {
        private CompleteQuestionnaireDocument innerDocument;
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return innerDocument;
        }
        public CompleteQuestionnaire(Questionnaire template, string userId)
        {
            innerDocument = new CompleteQuestionnaireDocument()
                                {
                                    Questionnaire = ((IEntity<QuestionnaireDocument>) template).GetInnerDocument(),
                                    UserId = userId,
                                    Status = 0,
                                    ResponsibleId = userId,
                                };
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
        }

        public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }

        public void ClearAnswers()
        {
            innerDocument.CompletedAnswers.Clear();
        }
        public void AddAnswer(CompleteAnswer answer)
        {
            if (innerDocument.CompletedAnswers.Where(a => a.PublicKey.Equals(answer.PublicKey)).Count() > 0)
                throw new DuplicateNameException("Answer with current public key already exists.");
            var templateAnswer =
                innerDocument.Questionnaire.Questions.Where(q=>q.PublicKey.Equals(answer.QuestionPublicKey)).SelectMany(q => q.Answers).Where(
                    a => a.PublicKey.Equals(answer.PublicKey)).FirstOrDefault();
            if (templateAnswer == null)
                throw new InvalidOperationException("Answer with current public key doesn't exist in question list.");
            if(!string.IsNullOrEmpty(answer.CustomAnswer) && templateAnswer.AnswerType!= AnswerType.Text)
                throw new InvalidOperationException("Only answer with type 'Text' can have custom text.");
            innerDocument.CompletedAnswers.Add(answer);
        }
        public void UpdateAnswerList(IEnumerable<CompleteAnswer> answers)
        {
            ClearAnswers();
            foreach (var answer in answers)
            {
                AddAnswer(answer);
            }
        }

        public Questionnaire GetQuestionnaireTemplate()
        {
            return new Questionnaire(innerDocument.Questionnaire);
        }

        public IList<CompleteAnswer> GetAllAnswers()
        {
            return innerDocument.CompletedAnswers;
        }
        public IList<Question> GetAllQuestions()
        {
            return innerDocument.Questionnaire.Questions;
        }


    }
}
