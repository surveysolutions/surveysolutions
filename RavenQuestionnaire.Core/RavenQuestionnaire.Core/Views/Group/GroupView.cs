#region

using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

#endregion

namespace RavenQuestionnaire.Core.Views.Group
{
    public abstract class AbstractGroupView : ICompositeView
    {
        public AbstractGroupView()
        {
            this.Children = new List<ICompositeView>();
        }

        public AbstractGroupView(string questionnaireId, Guid? parentGroup)
        {
            QuestionnaireKey = Guid.Parse(questionnaireId);
            Parent = parentGroup;
        }

        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            if (group.Triggers != null)
            {
                this.Trigger = group.Triggers.Count > 0 ? group.Triggers[0].ToString() : null;
            }
            else this.Trigger = null;
        }
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? Parent { get; set; }

        public string ConditionExpression { get; set; }

        public List<ICompositeView> Children { get; set; }

        public Propagate Propagated { get; set; }

        public string Trigger { get; set; }

        public Guid QuestionnaireKey
        {
            get; set; }

    }

    public abstract class AbstractGroupView<TGroup, TQuestion> : AbstractGroupView 
        where TGroup:AbstractGroupView
        where TQuestion : AbstractQuestionView
    {
        
        private TQuestion[] _questions;

        public AbstractGroupView()
        {
            Questions = new TQuestion[] { };
            Groups = new TGroup[] { };
        }

        public AbstractGroupView(Guid questionnaireId, Guid? parentGroup)
        {
            QuestionnaireKey = questionnaireId;
            Parent = parentGroup;
        }

        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireKey = doc.PublicKey;
            this.PublicKey = group.PublicKey;
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            if (group.Triggers != null)
            {
                this.Trigger = group.Triggers.Count>0 ? group.Triggers[0].ToString() : null;
            }
            else this.Trigger = null;
        }

        public TQuestion[] Questions
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

        public TGroup[] Groups { get; set; }


       
    }

    public abstract class GroupView<TGroupView, TQuestionView,TGroup, TQuestion> : AbstractGroupView<TGroupView, TQuestionView>
        where TGroupView : AbstractGroupView
        where TQuestionView : AbstractQuestionView
        where TQuestion : IQuestion
        where TGroup : IGroup
    {
        public GroupView()
        {
        }

        public GroupView(Guid questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        public GroupView(IQuestionnaireDocument doc, TGroup group)
            : base(doc, group)
        {
            this.Parent = GetGroupParent(doc, group);
        }
        protected Guid? GetGroupParent(IQuestionnaireDocument questionnaire, TGroup group)
        {
            if (questionnaire.Children.Any(q => q.PublicKey.Equals(group.PublicKey)))
                return null;
            var groups = new Queue<IComposite>();
            foreach (var child in questionnaire.Children)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                if(queueItem==null)
                    continue;

                if (queueItem.Children != null && queueItem.Children.Any(q => q.PublicKey.Equals(group.PublicKey)))
                    return queueItem.PublicKey;
                if (queueItem.Children != null)
                    foreach (var child in queueItem.Children)
                    {
                        groups.Enqueue(child);
                    }
            }
            return null;
        }
    }

    public class GroupView : GroupView<GroupView, QuestionView, IGroup,IQuestion>
    {
        public GroupView()
        {
        }

        public GroupView(Guid questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        public GroupView(
            IQuestionnaireDocument doc, IGroup group)
            : base(doc, group)
        {
            foreach (var composite in group.Children)
            {
                if ((composite as IQuestion) != null)
                {
                    var q = composite as IQuestion;
                    List<IQuestion> r = group.Children.OfType<IQuestion>().ToList();
                    this.Children.Add(new QuestionView(doc, q){ Index = r.IndexOf(q)+1 });
                }
                else
                {
                    var g = composite as IGroup;
                    this.Children.Add(new GroupView(doc, g));
                }
            }
            this.Questions =
                group.Children.OfType<IQuestion>().Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();
            this.Groups = group.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
            this.ConditionExpression = group.ConditionExpression;
        }

    }
}