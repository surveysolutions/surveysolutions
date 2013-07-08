using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.ExpressionExecutors;
using Main.Core.View.Group;

namespace Core.CAPI.Views.Json
{
    /// <summary>
    /// The complete questionnaire json view.
    /// </summary>
    public class CompleteQuestionnaireJsonView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireJsonView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, QuestionScope scope)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            this.CollectAll(doc, currentGroup as CompleteGroup, scope);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireJsonView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc, QuestionScope scope)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };
            this.CollectAll(doc, group, scope);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="CompleteQuestionnaireJsonView"/> class from being created.
        /// </summary>
        private CompleteQuestionnaireJsonView()
        {
            this.Questions = new List<CompleteQuestionsJsonView>();
            this.InnerGroups = new List<CompleteGroupMobileView>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the inner groups.
        /// </summary>
        public List<CompleteGroupMobileView> InnerGroups { get; set; }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        public CompleteGroupHeaders[] Menu { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        public List<CompleteQuestionsJsonView> Questions { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }


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
            List<ICompleteGroup> groups = doc.Children.OfType<ICompleteGroup>().ToList();

            this.Menu = new CompleteGroupHeaders[groups.Count];
            for (int i = 0; i < groups.Count; i++)
            {
                this.Menu[i] = new CompleteGroupHeaders(groups[i]);
            }
        }

       

        /// <summary>
        /// The collect all.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        private void CollectAll(CompleteQuestionnaireStoreDocument doc, CompleteGroup group, QuestionScope scope)
        {
            var queue = new Queue<ICompleteGroup>();
            queue.Enqueue(group);
            while (queue.Count != 0)
            {
                ICompleteGroup item = queue.Dequeue();
                if (item.IsGroupPropagationTemplate())
                {
                    continue;
                }

                Guid parentKey = item.PublicKey;
                bool propagatable = false;
                if (item.PropagationPublicKey.HasValue)
                {
                    parentKey = item.PropagationPublicKey.Value;
                    propagatable = true;
                }

                List<ICompleteQuestion> questions = item.Children.OfType<ICompleteQuestion>().ToList();
                foreach (ICompleteQuestion question in questions)
                {
                    this.Questions.Add(new CompleteQuestionsJsonView(question, parentKey, propagatable));
                }

                List<IComposite> innerGroups = item.Children.Where(c => c is ICompleteGroup).ToList();
                foreach (CompleteGroup g in innerGroups)
                {
                    queue.Enqueue(g);
                    this.InnerGroups.Add(new CompleteGroupMobileView(doc, g, scope));
                }
            }

            this.InitGroups(doc);
        }

      
        #endregion
    }
}