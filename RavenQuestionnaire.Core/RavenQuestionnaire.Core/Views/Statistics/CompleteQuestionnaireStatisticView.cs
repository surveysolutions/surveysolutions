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
            var nodes = new Queue<ICompleteGroup>(new List<ICompleteGroup> { target });
            var keys = new Queue<Guid>();
            keys.Enqueue(target.PublicKey);
            this.TotalQuestionCount = 0;
            {
                ICompleteGroup group = nodes.Dequeue();
                Guid key = keys.Dequeue();
                this.ProccessQuestions(
                    @group.Children.OfType<ICompleteQuestion>(), group.PublicKey, group.PropogationPublicKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(subGroup.PublicKey);
                }
            }

            while (nodes.Count > 0)
            {
                ICompleteGroup group = nodes.Dequeue();
                Guid key = keys.Dequeue();
                this.ProccessQuestions(
                    group.Children.OfType<ICompleteQuestion>(), group.PublicKey, group.PropogationPublicKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(key);
                }
            }

            this.CalculateApproximateAnswerTime(this.AnsweredQuestions);
        }

        /// <summary>
        /// The proccess questions.
        /// </summary>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        /// <param name="gropPropagationPublicKey">
        /// The grop propagation public key.
        /// </param>
        /// <param name="screenPublicKey">
        /// The screen public key.
        /// </param>
        protected void ProccessQuestions(
            IEnumerable<ICompleteQuestion> questions, 
            Guid gropPublicKey, 
            Guid? gropPropagationPublicKey, 
            Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                var statItem = new QuestionStatisticView(
                    completeQuestion, gropPublicKey, gropPropagationPublicKey, screenPublicKey);
                if (completeQuestion.Featured)
                {
                    this.FeaturedQuestions.Add(statItem);
                }

                if ((!completeQuestion.Valid)
                    || (completeQuestion.GetAnswerObject() == null && completeQuestion.Mandatory))
                {
                    this.InvalidQuestions.Add(statItem);
                }

                if (completeQuestion.GetAnswerObject() != null)
                {
                    this.AnsweredQuestions.Add(statItem);
                }

                this.TotalQuestionCount++;
            }
        }

        #endregion
    }
}