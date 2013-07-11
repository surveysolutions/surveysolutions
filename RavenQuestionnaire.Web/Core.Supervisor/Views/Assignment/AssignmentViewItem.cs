namespace Core.Supervisor.Views.Assignment
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The survey group item.
    /// </summary>
    public class AssignmentViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="AssignmentViewItem"/> class from being created.
        /// </summary>
        private AssignmentViewItem()
        {
            this.FeatureadValue = new Dictionary<Guid, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentViewItem"/> class.
        /// </summary>
        /// <param name="it">
        /// The it.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public AssignmentViewItem(CompleteQuestionnaireBrowseItem it, Dictionary<Guid, string> headers) : this()
        {
            this.Title = it.QuestionnaireTitle;
            this.Id = it.CompleteQuestionnaireId;
            this.Responsible = it.Responsible;
            this.TemplateId = it.TemplateId;
            this.LastEntryDate = it.LastEntryDate;
            this.Status = it.Status;
            foreach (var header in headers)
            {
                var question = it.FeaturedQuestions.FirstOrDefault(t => t.PublicKey == header.Key);
                this.FeatureadValue.Add(header.Key, question == null ? string.Empty : question.Answer.ToString());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentViewItem"/> class.
        /// </summary>
        /// <param name="it">
        /// The it.
        /// </param>
        public AssignmentViewItem(CompleteQuestionnaireBrowseItem it)
            : this()
        {
            this.Title = it.QuestionnaireTitle;
            this.Id = it.CompleteQuestionnaireId;
            this.Responsible = it.Responsible;
            this.TemplateId = it.TemplateId;
            this.LastEntryDate = it.LastEntryDate;
            this.Status = it.Status;
            if (it.FeaturedQuestions != null && it.FeaturedQuestions.Count() == 0)
            {
                this.FeatureadValue.Add(Guid.Empty, string.Empty);
            }
            else
            {
                this.FeatureadValue.Add(
                    Guid.Empty,
                    string.Join(
                        ", ",
                        it.FeaturedQuestions.Select(q => string.Format("{0} : {1}", q.Title, q.Answer.ToString())).
                            ToList()));
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the featuread value.
        /// </summary>
        public Dictionary<Guid, string> FeatureadValue { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets LastUpdate.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        #endregion
    }
}