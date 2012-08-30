// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireStatisticView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete questionnaire statistic view.
    /// </summary>
    public class CompleteQuestionnaireStatisticView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStatisticView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireStatisticView(CompleteQuestionnaireStoreDocument doc)
        {
            this.Id = doc.PublicKey.ToString();
            this.Title = doc.Title;
            this.StartDate = doc.CreationDate;
            this.EndDate = doc.CloseDate;
            this.CompleteQuestionnaireId = doc.PublicKey.ToString();
            this.Creator = doc.Creator;
            this.Status = doc.Status;
            this.HandleQuestionTree(doc);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answered questions.
        /// </summary>
        public IList<QuestionStatisticView> AnsweredQuestions { get; set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public string CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the featured questions.
        /// </summary>
        public IList<QuestionStatisticView> FeaturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the invalid questions.
        /// </summary>
        public IList<QuestionStatisticView> InvalidQuestions { get; set; }

        /// <summary>
        /// Gets or sets the invalid questions.
        /// </summary>
        public IList<QuestionStatisticView> UnansweredQuestions { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total question count.
        /// </summary>
        public int TotalQuestionCount { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The calculate approximate answer time.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        protected void CalculateApproximateAnswerTime(IList<QuestionStatisticView> list)
        {
            // todo optimizaton. current order by by could be optimized manualy
            List<QuestionStatisticView> unansweredList = list.Where(q => !q.AnswerDate.HasValue).ToList();
            list = list.Where(q => q.AnswerDate.HasValue).OrderBy(q => q.AnswerDate).ToList();
            if (list.Count > 0)
            {
                list[0].ApproximateTime = list[0].AnswerDate - this.StartDate;
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].ApproximateTime = list[i].AnswerDate - list[i - 1].AnswerDate;
                }
            }

            list.Union(unansweredList);
        }

        /// <summary>
        /// The handle question tree.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        protected void HandleQuestionTree(CompleteQuestionnaireStoreDocument target)
        {
            this.InvalidQuestions = new List<QuestionStatisticView>();
            this.AnsweredQuestions = new List<QuestionStatisticView>();
            this.FeaturedQuestions = new List<QuestionStatisticView>();
            this.UnansweredQuestions = new List<QuestionStatisticView>();
            foreach (var question in target.QuestionHash.WrapedQuestions)
            {
                this.ProccessQuestions(question.Question, question.GroupKey);
            }
            this.CalculateApproximateAnswerTime(this.AnsweredQuestions);
        }

        /// <summary>
        /// The proccess questions.
        /// </summary>
        /// <param name="question">
        /// The questions.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        protected void ProccessQuestions(ICompleteQuestion question, Guid gropPublicKey)
        {
            if (!question.Enabled)
                return;

            var statItem = new QuestionStatisticView(
                question, gropPublicKey);
            if (question.Featured)
            {
                this.FeaturedQuestions.Add(statItem);
            }

            if ((!question.Valid)
                || (question.GetAnswerObject() == null && question.Mandatory))
            {
                this.InvalidQuestions.Add(statItem);
            }

            if (question.GetAnswerObject() != null)
            {
                this.AnsweredQuestions.Add(statItem);
            }
            else
            {
                this.UnansweredQuestions.Add(statItem);
            }

            this.TotalQuestionCount++;
        }

        #endregion
    }
}