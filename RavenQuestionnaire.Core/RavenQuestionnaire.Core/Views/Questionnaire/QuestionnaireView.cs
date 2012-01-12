using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public QuestionView[] Questions
        {
            get { return _questions; }
            set
            {
                _questions = value;
                for (int i = 0; i < this._questions.Length; i++)
                {
                    this._questions[i].Index = i + 1;
                }

            }
        }

        public GroupView[] Groups { get; set; }
        private QuestionView[] _questions;
        public QuestionnaireView(IQuestionnaireDocument doc):this()
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Questions = new QuestionView[0];
            this.Groups=new GroupView[0];
        }
        public QuestionnaireView(IQuestionnaireDocument<RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question> doc)
            : this((IQuestionnaireDocument)doc)
        {
            this.Questions = doc.Questions.Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Groups.Select(g => new GroupView(doc, g)).ToArray();
        }
        public QuestionnaireView()
        {
            Questions = new QuestionView[0];
            Groups = new GroupView[0];
        }

        public QuestionView[] GetQuestions(Guid? groupPublicKey)
        {
            if (!groupPublicKey.HasValue)
                return Questions;
            var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(groupPublicKey.Value));
            if (group == null)
                throw new ArgumentException("group doesn't exists");
            return group.Questions;
        }
    }
}
