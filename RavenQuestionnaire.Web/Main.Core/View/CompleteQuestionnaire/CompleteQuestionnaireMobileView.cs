// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireMobileView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire mobile view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.ExpressionExecutors;
using Main.Core.View.Group;

namespace Main.Core.View.CompleteQuestionnaire
{
    /// <summary>
    /// The complete questionnaire mobile view.
    /// </summary>
    public class CompleteQuestionnaireMobileView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMobileView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireMobileView(CompleteQuestionnaireStoreDocument doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.Responsible = doc.Responsible;
            
            // CollectAll(doc, screenPublicKey, currentGroup as CompleteGroup, navigation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMobileView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="screenPublicKey">
        /// The screen public key.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        public CompleteQuestionnaireMobileView(
            CompleteQuestionnaireStoreDocument doc, 
            Guid screenPublicKey, 
            ICompleteGroup currentGroup, 
            ScreenNavigation navigation)
            : this(doc)
        {
            this.CollectAll(doc, screenPublicKey, currentGroup as CompleteGroup, navigation);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="CompleteQuestionnaireMobileView"/> class from being created.
        /// </summary>
        private CompleteQuestionnaireMobileView()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the current screen.
        /// </summary>
        public CompleteGroupMobileView CurrentScreen { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        public CompleteGroupHeaders[] Groups { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

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
        /// <param name="currentGroupPublicKey">
        /// The current group public key.
        /// </param>
        protected void InitGroups(CompleteQuestionnaireStoreDocument doc, Guid currentGroupPublicKey)
        {
            List<ICompleteQuestion> questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            List<ICompleteGroup> groups = doc.Children.OfType<ICompleteGroup>().ToList();
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            if (questions.Count > 0)
            {
                this.Groups = new CompleteGroupHeaders[groups.Count + 1];

                this.Groups[0] = new CompleteGroupHeaders
                    {
                       PublicKey = Guid.Empty, GroupText = "Main", Totals = this.CountQuestions(questions), Description = string.Empty
                    };
                for (int i = 1; i <= groups.Count; i++)
                {
                    this.Groups[i] = new CompleteGroupHeaders
                        {
                            PublicKey = groups[i - 1].PublicKey, 
                            GroupText = groups[i - 1].Title, 
                            Enabled = executor.Execute(groups[i - 1]),
                            Description = groups[i - 1].Description
                        };
                    this.Groups[i].Totals = this.CalcProgress(groups[i - 1]);
                }
            }
            else
            {
                this.Groups = new CompleteGroupHeaders[groups.Count];
                for (int i = 0; i < groups.Count; i++)
                {
                    this.Groups[i] = new CompleteGroupHeaders
                        {
                            PublicKey = groups[i].PublicKey, 
                            GroupText = groups[i].Title, 
                            Enabled = executor.Execute(groups[i]),
                            Description = groups[i].Description
                        };
                    this.Groups[i].Totals = this.CalcProgress(groups[i]);
                }
            }

            CompleteGroupHeaders current = this.Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
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
        /// <param name="screenPublicKey">
        /// The screen public key.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        private void CollectAll(
            CompleteQuestionnaireStoreDocument doc, 
            Guid screenPublicKey, 
            CompleteGroup group, 
            ScreenNavigation navigation)
        {
            var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
            executor.Execute(group);
            var currentGroup = new CompleteGroupMobileView(doc, group, navigation);
            this.InitGroups(doc, screenPublicKey);
            this.Totals = this.CalcProgress(doc);
            this.CurrentScreen = currentGroup.Propagated != Propagate.None
                                     ? currentGroup.PropagateTemplate
                                     : currentGroup;
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
                    Answered = enabled.Count(question => question.GetAnswerObject() != null)
                };
            return total;
        }

        #endregion
    }
}