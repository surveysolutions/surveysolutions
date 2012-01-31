#region

using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

#endregion

namespace RavenQuestionnaire.Core.Views.Group
{
    public abstract class AbstractGroupView
    {
        private string _questionnaireId;
        private AbstractQuestionView[] _questions;

        public AbstractGroupView()
        {
            Questions = new AbstractQuestionView[] { };
            Groups = new AbstractGroupView[] { };
        }

        public AbstractGroupView(string questionnaireId, Guid? parentGroup)
        {
            QuestionnaireId = questionnaireId;
            ParentGroup = parentGroup;
        }

        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.GroupText = group.Title;
            this.Propagated = group.Propagated;
        }
        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public Guid? ParentGroup { get; set; }

        public bool Propagated { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

        public AbstractQuestionView[] Questions
        {
            get { return _questions; }
            set
            {
                _questions = value;
                for (var i = 0; i < _questions.Length; i++)
                {
                    _questions[i].Index = i + 1;
                }
            }
        }

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

        public GroupView(string questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        public GroupView(IQuestionnaireDocument<TGroup, TQuestion> doc, TGroup group)
            : base(doc, group)
        {
            this.ParentGroup = GetGroupParent(doc, group);
            /*   this.Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();*/
        }
        protected Guid? GetGroupParent(IQuestionnaireDocument<TGroup, TQuestion> questionnaire, TGroup group)
        {
            if (questionnaire.Groups.Any(q => q.PublicKey.Equals(group.PublicKey)))
                return null;
            var groups = new Queue<TGroup>();
            foreach (var child in questionnaire.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();

                if (queueItem.Groups.Any(q => q.PublicKey.Equals(group.PublicKey)))
                    return queueItem.PublicKey;
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
            return null;
        }
    }

    public class GroupView : GroupView<Entities.SubEntities.Group, Entities.SubEntities.Question, Entities.SubEntities.Answer>
    {
        public GroupView()
        {
        }

        public GroupView(string questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        public GroupView(
            IQuestionnaireDocument<Entities.SubEntities.Group, Entities.SubEntities.Question> doc, Entities.SubEntities.Group group)
            : base(doc, group)
        {
            Questions =
                group.Questions.Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();
            this.Groups = group.Groups.Select(g => new GroupView(doc, g)).ToArray();
        }
    }
}