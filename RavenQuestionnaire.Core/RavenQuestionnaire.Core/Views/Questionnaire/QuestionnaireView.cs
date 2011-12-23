using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
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

        protected QuestionView[] Questions
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
        public QuestionnaireView(QuestionnaireDocument doc)
            : this()
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Questions = doc.Questions.Select(q => new QuestionView(doc.Id, q)).ToArray();
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
