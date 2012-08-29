// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaire.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete questionnaire.
    /// </summary>
    public class CompleteQuestionnaire : IEntity<CompleteQuestionnaireDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly CompleteQuestionnaireDocument innerDocument;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaire"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public CompleteQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.innerDocument = document;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaire"/> class.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="completeQuestionnaireGuid">
        /// The complete questionnaire guid.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        public CompleteQuestionnaire(
            Questionnaire template, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status)
        {
            this.innerDocument =
                (CompleteQuestionnaireDocument)((IEntity<QuestionnaireDocument>)template).GetInnerDocument();
            this.innerDocument.PublicKey = completeQuestionnaireGuid;
            this.innerDocument.Creator = user;
            this.innerDocument.Status = status;
            this.innerDocument.Responsible = user;
        }

        #endregion

        /*public string CompleteQuestinnaireId
        {
            get { return innerDocument.Id; }
        }*/
        #region Public Properties

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey
        {
            get
            {
                return this.innerDocument.PublicKey;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public virtual void Add(IComposite c, Guid? parent)
        {
            var group = c as CompleteGroup;
            if (group != null && group.Propagated != Propagate.None)
            {
                if (!group.PropogationPublicKey.HasValue)
                {
                    c = new CompleteGroup(group, Guid.NewGuid());
                }
            }

            this.innerDocument.Add(c, parent);
            this.UpdateLastEntryDate();
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.innerDocument.Find<T>(publicKey);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return this.innerDocument.Find(condition);
        }

        /// <summary>
        /// The first or default.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.innerDocument.FirstOrDefault(condition);
        }

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.CompleteQuestionnaireDocument.
        /// </returns>
        public CompleteQuestionnaireDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        public void Remove(IComposite c)
        {
            this.innerDocument.Remove(c);
            this.UpdateLastEntryDate();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            this.innerDocument.Remove(publicKey);
            this.UpdateLastEntryDate();
        }

        /// <summary>
        /// The set responsible.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public void SetResponsible(UserLight user)
        {
            this.innerDocument.Responsible = user;
        }

        /// <summary>
        /// The set status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        public void SetStatus(SurveyStatus status)
        {
            this.innerDocument.Status = status;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update last entry date.
        /// </summary>
        protected void UpdateLastEntryDate()
        {
            this.innerDocument.LastEntryDate = DateTime.Now;
        }

        #endregion
    }
}