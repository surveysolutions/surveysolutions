using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>, IComposite//, IPropogate
    {
        private CompleteQuestionnaireDocument innerDocument;
        private CompositeHandler handler;
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
            handler = new CompositeHandler(innerDocument.Observers, this);
            
            /* foreach (ICompleteGroup completeGroup in GroupIterator)
             {
                 var group = completeGroup as CompleteGroup;
                 if (group != null)
                     group.Subscribe(this.handler);
             }*/
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
            handler = new CompositeHandler(innerDocument.Observers, this);
            /*    foreach (ICompleteGroup completeGroup in GroupIterator)
                {
                    var group = completeGroup as CompleteGroup;
                    if (group != null)
                        group.Subscribe(this.handler);
                }*/
        }

        public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }
        /*  public void Subscribe(IComposite target, CompleteGroup group, Actions action)
          {
              group.Subscribe(this.handler);
          }
          public void Unsubscribe(CompleteGroup group)
          {
              group.Unsubscribe();
          }*/
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
            this.handler.Add(c);

        }

        public void Remove(IComposite c)
        {
            innerDocument.Remove(c);
            this.handler.Remove(c);
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            innerDocument.Remove<T>(publicKey);
            this.handler.Remove<T>(publicKey);
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

        #endregion

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            return innerDocument.Subscribe(observer);
        }

        #endregion
    }
}
