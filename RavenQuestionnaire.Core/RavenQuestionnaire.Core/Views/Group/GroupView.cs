#region

using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;

#endregion

namespace RavenQuestionnaire.Core.Views.Group
{
    public abstract class AbstractGroupView : ICompositeView
    {
        private string _questionnaireId;

        public AbstractGroupView()
        {
            this.Children = new List<ICompositeView>();
        }

        public AbstractGroupView(string questionnaireId, Guid? parentGroup)
        {
            QuestionnaireId = questionnaireId;
            Parent = parentGroup;
        }

        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.Title = group.Title;
            this.Propagated = group.Propagated;
        }
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? Parent { get; set; }

        public List<ICompositeView> Children { get; set; }

        public Propagate Propagated { get; set; }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

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

        public AbstractGroupView(string questionnaireId, Guid? parentGroup)
        {
            QuestionnaireId = questionnaireId;
            Parent = parentGroup;
        }

        protected AbstractGroupView(IQuestionnaireDocument doc, IGroup group)
        {
            this.QuestionnaireId = doc.Id;
            this.PublicKey = group.PublicKey;
            this.Title = group.Title;
            this.Propagated = group.Propagated;
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

        public GroupView(string questionnaireId, Guid? parentGroup)
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

        public GroupView(string questionnaireId, Guid? parentGroup)
            : base(questionnaireId, parentGroup)
        {
        }

        public GroupView(
            IQuestionnaireDocument doc, IGroup group)
            : base(doc, group)
        {
            //foreach (var composite in doc.Children)
            //{
            //    if ((composite as IQuestion) != null)
            //    {
            //        var q = composite as ICompleteQuestion;
            //        var question = new CompleteQuestionFactory().CreateQuestion(doc as CompleteQuestionnaireDocument, null, q);
            //        this.Children.Add(question);
            //    }
            //    else
            //    {
            //        var g = composite as IGroup;
            //        this.Children.Add(new GroupView(doc, g));

            //    }
            //}
            this.Questions =
                group.Children.OfType<IQuestion>().Select(
                    q =>
                    new QuestionView(doc, q)).ToArray();
            this.Groups = group.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
        }

    }
}