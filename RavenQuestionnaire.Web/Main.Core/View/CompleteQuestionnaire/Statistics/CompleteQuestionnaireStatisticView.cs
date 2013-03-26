// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireStatisticView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.View.CompleteQuestionnaire.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;

    /// <summary>
    /// The complete questionnaire statistic view.
    /// </summary>
    public class CompleteQuestionnaireStatisticView
    {
        #region Fields

        /*/// <summary>
        /// The executor.
        /// </summary>
        protected readonly CompleteQuestionnaireConditionExecutor executor;*/

        /// <summary>
        /// The validator.
        /// </summary>
        protected readonly CompleteQuestionnaireValidationExecutor validator;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStatisticView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireStatisticView(CompleteQuestionnaireStoreDocument doc, QuestionScope scope)
        {
            // moved to the wright layer
            // this.executor = new CompleteQuestionnaireConditionExecutor(doc);

            this.validator = new CompleteQuestionnaireValidationExecutor(doc, scope);

            this.Id = doc.PublicKey.ToString();
            this.Title = doc.Title;
            this.StartDate = doc.CreationDate;
            this.EndDate = doc.CloseDate;
            this.CompleteQuestionnaireId = doc.PublicKey.ToString();
            this.Creator = doc.Creator;
            this.Status = doc.Status;
            this.LastScreenPublicKey = doc.Children.OfType<ICompleteGroup>().Last().PublicKey;
            this.StatusHistory = doc.StatusChangeComments.Select(s => new ChangeStatusHistoryView(s.Responsible, s.Status, s.ChangeDate)).ToList();
            this.StatusHistory.Reverse();
            this.HandleQuestionTree(doc, scope);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets StatusHistory.
        /// </summary>
        public List<ChangeStatusHistoryView> StatusHistory { get; set; }

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
        public DateTime StartDate { get; set; }

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
        /// Gets or sets the last screen.
        /// </summary>
        public Guid LastScreenPublicKey { get; set; }



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

        /// <summary>
        /// Gets or sets the invalid questions.
        /// </summary>
        public IList<QuestionStatisticView> UnansweredQuestions { get; set; }

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
        protected void HandleQuestionTree(CompleteQuestionnaireStoreDocument target, QuestionScope scope)
        {
            this.InvalidQuestions = new List<QuestionStatisticView>();
            this.AnsweredQuestions = new List<QuestionStatisticView>();
            this.FeaturedQuestions = new List<QuestionStatisticView>();
            this.UnansweredQuestions = new List<QuestionStatisticView>();

            // moved to the write layer
            // this.executor.Execute(target);

            foreach (var question in target.WrappedQuestions().Where(q=>q.Question.QuestionScope <= scope))
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
            ////question.Enabled = this.executor.Execute(question) ?? true;

            if (!question.Enabled)
            {
                return;
            }

            question.Valid = this.validator.Execute(question);

            var statItem = new QuestionStatisticView(question, gropPublicKey);
            if (question.Featured)
            {
                this.FeaturedQuestions.Add(statItem);
            }

            if (!question.Valid)
            {
                this.InvalidQuestions.Add(statItem);
            }

            if (question.IsAnswered())
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