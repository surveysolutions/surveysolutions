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
    public abstract class AbstractGroupView
    {
        public AbstractGroupView()
        {
            Questions = new AbstractQuestionView[] { };
            Groups = new AbstractGroupView[] { };
        }
        public AbstractGroupView(string questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }
        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.GroupText;
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

        public AbstractQuestionView[] Questions
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
        private AbstractQuestionView[] _questions;

        public AbstractGroupView[] Groups { get; set; }
    }

    public abstract class GroupView<TGroup, TQuestion, TAnswer> : AbstractGroupView
        where TAnswer : IAnswer
        where TQuestion : IQuestion<TAnswer>
        where TGroup : IGroup<TGroup, TQuestion>
    {
        public GroupView()
        {
        }

        public GroupView(string questionnaireId)
            : base(questionnaireId)
        {
        }

        public GroupView(IQuestionnaireDocument<TGroup, TQuestion> doc, TGroup group)
            : base(doc, group)
        {
         /*   this.Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();*/
        }
    }

    public class GroupView :
        GroupView
            <RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question,
            RavenQuestionnaire.Core.Entities.SubEntities.Answer>
    {
        public GroupView()
        {
        }

        public GroupView(string questionnaireId)
            : base(questionnaireId)
        {
        }

        public GroupView(
            IQuestionnaireDocument
                <RavenQuestionnaire.Core.Entities.SubEntities.Group,
                RavenQuestionnaire.Core.Entities.SubEntities.Question> doc,
            RavenQuestionnaire.Core.Entities.SubEntities.Group group)
            : base(doc, group)
        {
            this.Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();
        }
    }
}
