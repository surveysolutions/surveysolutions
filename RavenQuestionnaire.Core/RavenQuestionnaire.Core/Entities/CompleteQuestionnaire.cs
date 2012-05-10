using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ninject;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>
    {
        private CompleteQuestionnaireDocument innerDocument;
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return innerDocument;
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
        }
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status)
        {

            innerDocument =
           (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>)template).GetInnerDocument();
            innerDocument.Creator = user;
            innerDocument.Status = status;
            innerDocument.Responsible = user;
        }
        public CompleteQuestionnaire(Questionnaire template, UserLight user, SurveyStatus status, ISubscriber subscriber):this(template,user,status)
        {
            subscriber.Subscribe(this.innerDocument);
        }

        public CompleteQuestionnaire(CompleteQuestionnaireDocument document, ISubscriber subscriber)
            : this(document)
        {

            subscriber.Subscribe(this.innerDocument);
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
        protected void UpdateLastEntryDate()
        {
            this.innerDocument.LastEntryDate = DateTime.Now;

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
            UpdateLastEntryDate();
        }

        public void Remove(IComposite c)
        {
            innerDocument.Remove(c);
            UpdateLastEntryDate();
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            innerDocument.Remove(publicKey);
            UpdateLastEntryDate();
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
