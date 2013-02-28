// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimelineView.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The complete questionnaire statistic view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Timeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.View.Question;

    using ChangeStatusHistoryView = Core.Supervisor.Views.Survey.ChangeStatusHistoryView;

    /// <summary>
    /// The complete questionnaire statistic view.
    /// </summary>
    public class TimelineView
    {
        #region Constants and Fields

        /// <summary>
        /// The validator.
        /// </summary>
        protected readonly CompleteQuestionnaireValidationExecutor validator;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public TimelineView(CompleteQuestionnaireStoreDocument doc)
        {
            this.Events = new List<TimelineEvent>();
            this.validator = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Headquarter);

            this.Id = doc.PublicKey.ToString();
            this.Title = doc.Title;
            this.StartDate = doc.CreationDate;
            this.EndDate = doc.CloseDate;
            this.CompleteQuestionnaireId = doc.PublicKey.ToString();
            this.Creator = doc.Creator;
            this.Status = doc.Status;
            this.StatusHistory =
                doc.StatusChangeComments.Select(s => new ChangeStatusHistoryView(s.Responsible, s.Status, s.ChangeDate))
                    .ToList();
            this.StatusHistory.Reverse();
            this.HandleQuestionTree(doc);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public string CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the answered questions.
        /// </summary>
        public IList<TimelineEvent> Events { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets StatusHistory.
        /// </summary>
        public List<ChangeStatusHistoryView> StatusHistory { get; set; }

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
        protected void CalculateApproximateAnswerTime(IList<TimelineEvent> list)
        {
            // todo optimizaton. current order by by could be optimized manualy
            list = list.Where(q => q.Question.AnswerDate.HasValue).OrderBy(q => q.Question.AnswerDate).ToList();
            if (list.Count > 0)
            {
                list[0].Question.ApproximateTime = list[0].Question.AnswerDate - this.StartDate;
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].Question.ApproximateTime = list[i].Question.AnswerDate - list[i - 1].Question.AnswerDate;
                }
            }
        }

        /// <summary>
        /// The handle question tree.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        protected void HandleQuestionTree(CompleteQuestionnaireStoreDocument target)
        {
            foreach (CompleteQuestionWrapper question in target.WrappedQuestions())
            {
                this.ProccessQuestions(target, question.Question, question.GroupKey);
            }

            this.CalculateApproximateAnswerTime(this.Events);
        }

        /// <summary>
        /// The proccess questions.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="question">
        /// The questions.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        protected void ProccessQuestions(
            CompleteQuestionnaireStoreDocument doc, ICompleteQuestion question, Guid gropPublicKey)
        {
            if (!question.Enabled)
            {
                return;
            }

            question.Valid = this.validator.Execute(question);

            if (!question.AnswerDate.HasValue || !question.IsAnswered())
            { 
                return;
            }

            this.Events.Add(new TimelineEvent(question, new CompleteQuestionView(doc, question)));

            this.TotalQuestionCount++;
        }

        #endregion
    }
}