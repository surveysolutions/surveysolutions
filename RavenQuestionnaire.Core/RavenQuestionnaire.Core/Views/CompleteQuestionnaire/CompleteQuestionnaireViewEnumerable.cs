// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireViewEnumerable.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire view enumerable.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.Group;

    /// <summary>
    /// The complete questionnaire view enumerable.
    /// </summary>
    public class CompleteQuestionnaireViewEnumerable
    {
        #region Fields

        /// <summary>
        /// The group factory.
        /// </summary>
        protected readonly ICompleteGroupFactory GroupFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewEnumerable"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="groupFactory">
        /// The group factory.
        /// </param>
        public CompleteQuestionnaireViewEnumerable(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ICompleteGroupFactory groupFactory)
        {
            this.GroupFactory = groupFactory;
            this.Id = doc.PublicKey.ToString();
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            this.InitGroups(doc);
            this.CurrentGroup = CompleteGroupView.CreateGroup(doc, currentGroup, this.GroupFactory);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewEnumerable"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="groupFactory">
        /// The group factory.
        /// </param>
        public CompleteQuestionnaireViewEnumerable(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroupFactory groupFactory)
        {
            this.GroupFactory = groupFactory;
            this.LastEntryDate = doc.LastEntryDate;
            this.Title = doc.Title;
            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };
            this.CurrentGroup = CompleteGroupView.CreateGroup(doc, group, this.GroupFactory);
            this.InitGroups(doc);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the current group.
        /// </summary>
        public CompleteGroupView CurrentGroup { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        public CompleteGroupView[] Groups { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The init groups.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected void InitGroups(CompleteQuestionnaireStoreDocument doc)
        {
            List<ICompleteQuestion> questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            List<ICompleteGroup> groups = doc.Children.OfType<ICompleteGroup>().ToList();
            if (questions.Count > 0)
            {
                this.Groups = new CompleteGroupView[groups.Count + 1];
                this.Groups[0] = CompleteGroupView.CreateGroup(
                    doc, new CompleteGroup("Main") { PublicKey = Guid.Empty }, this.GroupFactory);
                for (int i = 1; i <= groups.Count; i++)
                {
                    this.Groups[i] = CompleteGroupView.CreateGroup(doc, groups[i - 1], this.GroupFactory);
                }
            }
            else
            {
                this.Groups = new CompleteGroupView[groups.Count];
                for (int i = 0; i < groups.Count; i++)
                {
                    this.Groups[i] = CompleteGroupView.CreateGroup(doc, groups[i], this.GroupFactory);
                }
            }
        }

        #endregion
    }
}