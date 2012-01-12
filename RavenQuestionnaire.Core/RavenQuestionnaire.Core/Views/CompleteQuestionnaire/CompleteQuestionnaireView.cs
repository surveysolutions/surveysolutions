using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireView
    {
        public string Id { get; set; }
        public string Title
        {
            get { return Questionnaire.Title; }
        }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }

        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }

        private CompleteQuestionView[] _questions;

        private QuestionnaireView Questionnaire { get; set; }

        public CompleteQuestionnaireView(CompleteQuestionnaireDocument doc)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Questionnaire = new QuestionnaireView(doc);
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            //TODO _question may be redundant
            _questions =
                doc.Questions.Select(q => new CompleteQuestionView(q, doc.Id)).ToArray();

        }
        public CompleteQuestionnaireView(QuestionnaireDocument template, bool exposeFields)
        {
            this.Questionnaire = new QuestionnaireView(template);
            _questions =
                template.Questions.Select(
                    q => new CompleteQuestionView(new CompleteQuestion(q.QuestionText, q.QuestionType), template.Id)).
                    ToArray();
        }
    }
}
