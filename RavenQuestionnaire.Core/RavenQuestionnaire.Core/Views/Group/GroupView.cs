using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupView
    {
        public GroupView()
        {
            Questions = new QuestionView[] {};
            Groups = new GroupView[] {};
        }
        public GroupView(string questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }
        protected GroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
          /*  this.Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();*/
        }
        public GroupView(IQuestionnaireDocument<RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question> doc, RavenQuestionnaire.Core.Entities.SubEntities.Group group)
            : this(doc, (IGroup)group)
        {
            this.Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();
        }

        public Guid PublicKey { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroup { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }
        private string _questionnaireId;
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

        private QuestionView[] _questions;
        public GroupView[] Groups { get; set; }
    }
}
