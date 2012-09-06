// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireJsonView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire json view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.ExpressionExecutors;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

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
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            this.CollectAll(doc, currentGroup as CompleteGroup);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireJsonView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };
            this.CollectAll(doc, group);
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

        /// <summary>
        /// Gets or sets the totals.
        /// </summary>
        public Counter Totals { get; set; }

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
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            for (int i = 0; i < groups.Count; i++)
            {
                this.Menu[i] = new CompleteGroupHeaders
                    {
                        PublicKey = groups[i].PublicKey, 
                        GroupText = groups[i].Title, 
                        Enabled = executor.Execute(groups[i])
                    };
                this.Menu[i].Totals = this.CalcProgress(groups[i]);
            }
        }

        /// <summary>
        /// The calc progress.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Counter.
        /// </returns>
        private Counter CalcProgress(ICompleteGroup @group)
        {
            var total = new Counter();
            if (@group.PropogationPublicKey.HasValue)
            {
                total = total + this.CountQuestions(@group.Children.Select(q => q as ICompleteQuestion).ToList());
                return total;
            }

            var complete = @group as CompleteGroup;
            if (complete != null && complete.Propagated != Propagate.None)
            {
                return total;
            }

            List<ICompleteGroup> gruoSubGroup = @group.Children.OfType<ICompleteGroup>().ToList();
            List<ICompleteQuestion> gruoSubQuestions = @group.Children.OfType<ICompleteQuestion>().ToList();
            total = total + this.CountQuestions(gruoSubQuestions);
            foreach (ICompleteGroup g in gruoSubGroup)
            {
                total = total + this.CalcProgress(g);
            }

            return total;
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
        private void CollectAll(CompleteQuestionnaireStoreDocument doc, CompleteGroup group)
        {
            var queue = new Queue<ICompleteGroup>();
            queue.Enqueue(group);
            while (queue.Count != 0)
            {
                ICompleteGroup item = queue.Dequeue();
                if (!item.PropogationPublicKey.HasValue && item.Propagated == Propagate.Propagated)
                {
                    continue;
                }

                Guid parentKey = item.PublicKey;
                bool propagatable = false;
                if (item.PropogationPublicKey.HasValue)
                {
                    parentKey = item.PropogationPublicKey.Value;
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
                    this.InnerGroups.Add(new CompleteGroupMobileView(doc, g, null));
                }
            }

            this.InitGroups(doc);
            this.Totals = this.CalcProgress(doc);
        }

        /// <summary>
        /// The count questions.
        /// </summary>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Counter.
        /// </returns>
        private Counter CountQuestions(List<ICompleteQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return new Counter();
            }

            List<ICompleteQuestion> enabled = questions.Where(q => q.Enabled).ToList();
            var total = new Counter
                {
                    Total = questions.Count, 
                    Enablad = enabled.Count(), 
                    Answered = enabled.Count(question => question.GetAnswerObject() != null),
                    Invalid = enabled.Count(question => !question.Valid)
                };
            return total;
        }

        #endregion
    }
}