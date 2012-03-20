using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite//, IPropogate
    {
        private CompleteQuestionnaireDocument innerDocument;
       // private CompositeHandler handler;
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return innerDocument;
        }
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status)
        {
            innerDocument =
                (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>)template).GetInnerDocument();
            innerDocument.Creator = user;
            innerDocument.Status = status;
            innerDocument.Responsible = user;
            
            /* foreach (ICompleteGroup completeGroup in GroupIterator)
             {
                 var group = completeGroup as CompleteGroup;
                 if (group != null)
                     group.Subscribe(this.handler);
             }*/
            SubscibeAutoPropogation();
            SubscribeBindedQuestions();
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
            SubscibeAutoPropogation();
            SubscribeBindedQuestions();
        }
        protected void SubscibeAutoPropogation()
        {
            innerDocument.GetGroupPropagatedEvents().Subscribe(Observer.Create<CompositeAddedEventArgs>(AutoPropagate));
            innerDocument.GetGroupPropagatedRemovedEvents().Subscribe(Observer.Create<CompositeRemovedEventArgs>(RemoveAutoPropagate));
        }
        protected void SubscribeBindedQuestions()
        {
            var addAnswers = from q in this.GetAllAnswerAddedEvents()
                             let question =
                                 ((CompositeAddedEventArgs)q.ParentEvent).AddedComposite as
                                 ICompleteQuestion
                             let binded =
                                 this.innerDocument.GetAllBindedQuestions(question.PublicKey)
                             where binded.Any()
                             select q;
            addAnswers
                .Subscribe(Observer.Create<CompositeAddedEventArgs>(
                    BindQuestion));
        }
        protected void BindQuestion(CompositeAddedEventArgs e)
        {
            var template = ((CompositeAddedEventArgs)e.ParentEvent).AddedComposite as ICompleteQuestion;

            if (template == null)
                return;
            var propagatedTemplate = template as IPropogate;
            IEnumerable<BindedCompleteQuestion> binded;
            if (propagatedTemplate == null)
            {
                binded =
                    this.innerDocument.GetAllBindedQuestions(template.PublicKey);
            }
            else
            {
                binded = this.innerDocument.GetPropagatedGroupsByKey(propagatedTemplate.PropogationPublicKey).SelectMany(
                    pg => pg.GetAllBindedQuestions(template.PublicKey));
            }
            foreach (BindedCompleteQuestion bindedCompleteQuestion in binded)
            {
                bindedCompleteQuestion.Copy(template);
            }

        }

        protected void AutoPropagate(CompositeAddedEventArgs e)
        {
           // ICompleteGroup template = ((CompositeAddedEventArgs) e.ParentEvent).AddedComposite as ICompleteGroup; 
            PropagatableCompleteGroup group = e.AddedComposite as PropagatableCompleteGroup;
            if(group==null)
                return;
            var triggeres =
                this.Find<ICompleteGroup>(
                    g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
            foreach (ICompleteGroup triggere in triggeres)
            {
                var propagatebleGroup = new PropagatableCompleteGroup(triggere, group.PropogationPublicKey);
                innerDocument.Add(propagatebleGroup, null);
            }
        }
       
        protected void RemoveAutoPropagate(CompositeRemovedEventArgs e)
        {
            PropagatableCompleteGroup group = e.RemovedComposite as PropagatableCompleteGroup;
            if (group == null)
                return;
              var triggeres =
                this.Find<ICompleteGroup>(
                    g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
              foreach (ICompleteGroup triggere in triggeres)
              {
                  var propagatebleGroup = new PropagatableCompleteGroup(triggere, group.PropogationPublicKey);
                  innerDocument.Remove(propagatebleGroup);
              }
        }
        public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }
        public void SetStatus(SurveyStatus status)
        {
            innerDocument.Status = status;
        }


        public void SetResponsible(UserLight user)
        {
            innerDocument.Responsible = user;
        }
        #region Implementation of IComposite

        public Guid PublicKey
        {
            get { return innerDocument.PublicKey; }
        }

        public virtual void Add(IComposite c, Guid? parent)
        {
            CompleteGroup group = c as CompleteGroup;
            if (group != null && group.Propagated != Propagate.None)
            {
                if (!(group is PropagatableCompleteGroup))
                    c = new PropagatableCompleteGroup(group, Guid.NewGuid());

            }
            innerDocument.Add(c, parent);

        }

        public void Remove(IComposite c)
        {
            innerDocument.Remove(c);
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            innerDocument.Remove<T>(publicKey);
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return innerDocument.Find<T>(publicKey);
        }
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                innerDocument.Find<T>(condition);

        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return innerDocument.FirstOrDefault<T>(condition);
        }

        #endregion

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            return innerDocument.Subscribe(observer);
        }

        #endregion
    }
}
