using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

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
               // throw new InvalidOperationException("can't be bellow zero");
        }
        public CompleteQuestionnaire(Questionnaire template, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status)
        {

            innerDocument = (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>)template).GetInnerDocument();
            innerDocument.PublicKey = completeQuestionnaireGuid;
            innerDocument.Creator = user;
            innerDocument.Status = status;
            innerDocument.Responsible = user;
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
                if (!group.PropogationPublicKey.HasValue)
                    c = new CompleteGroup(group, Guid.NewGuid());

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

    
    }
}
