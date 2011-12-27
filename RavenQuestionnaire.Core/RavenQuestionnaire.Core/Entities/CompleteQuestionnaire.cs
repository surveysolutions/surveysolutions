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
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status)
        {
            innerDocument = new CompleteQuestionnaireDocument()
                                {
                                    Questionnaire = ((IEntity<QuestionnaireDocument>) template).GetInnerDocument(),
                                    Creator = user,
                                    Status = status,
                                    Responsible = user,
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
        public void AddAnswer(CompleteAnswer answer, Guid? groupPublicKey)
        {
            if(answer.PublicKey== Guid.Empty)
                return;
            if (innerDocument.CompletedAnswers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                throw new DuplicateNameException("Answer with current public key already exists.");

            var templateAnswer = this.GetQuestionsFromGroup(groupPublicKey).
                Where(q=>q.PublicKey.Equals(answer.QuestionPublicKey)).
                SelectMany(q => q.Answers).
                FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));

            if (templateAnswer == null)
                throw new InvalidOperationException("Answer with current public key doesn't exist in question list.");
           /* if(!string.IsNullOrEmpty(answer.CustomAnswer) && templateAnswer.AnswerType!= AnswerType.Text)
                throw new InvalidOperationException("Only answer with type 'Text' can have custom text.");*/
            answer.CustomAnswer = templateAnswer.AnswerText;
            innerDocument.CompletedAnswers.Add(answer);
        }
        public void UpdateAnswer(CompleteAnswer answer, Guid? groupPublicKey)
        {
            var question = this.GetQuestionsFromGroup(groupPublicKey).FirstOrDefault(q => q.PublicKey.Equals(answer.QuestionPublicKey));

            if (question == null)
                throw new InvalidOperationException("Question does not exist in questionnaire");

            innerDocument.CompletedAnswers.RemoveAll(a => a.QuestionPublicKey == question.PublicKey);
            AddAnswer(answer, groupPublicKey);
        }

        public void SetStatus(SurveyStatus status)
        {
            innerDocument.Status = status;
        }


        public void SetResponsible(UserLight user)
        {
            innerDocument.Responsible = user;
        }


        protected List<Question> GetQuestionsFromGroup(Guid? groupPublicKey)
        {
            Questionnaire questionnaire = new Questionnaire(innerDocument.Questionnaire);
            List<Question> questions;
            if (groupPublicKey.HasValue)
            {
                Group group = questionnaire.Find<Group>(groupPublicKey.Value);
                if (group == null)
                    throw new ArgumentException("group does not exsit");
                questions = group.Questions;
            }
            else
            {
                questions = innerDocument.Questionnaire.Questions;
            }
            return questions;
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
        public IList<Group> GetAllGroups()
        {
            return innerDocument.Questionnaire.Groups;
        }


    }
}
