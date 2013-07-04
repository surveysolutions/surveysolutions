using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.View.CompleteQuestionnaire;

namespace Core.Supervisor.Views.Interviewer
{
    using Core.Supervisor.Views.Survey;

    /// <summary>
    /// The interviewer item view.
    /// </summary>
    public class InterviewerItemView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerItemView"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="featuredHeaders">
        /// The featured headers.
        /// </param>
        public InterviewerItemView(CompleteQuestionnaireBrowseItem item, Dictionary<Guid, string> featuredHeaders)
        {
            this.TemplateId = item.TemplateId;
            this.Status = item.Status;
            this.FeaturedQuestions = new Dictionary<Guid, string>();
            foreach (var kvp in featuredHeaders.OrderBy(t => t.Key))
            {
                var featured = item.FeaturedQuestions.FirstOrDefault(q => q.PublicKey == kvp.Key);
                this.FeaturedQuestions.Add(kvp.Key, featured == null ? string.Empty : featured.Answer.ToString());
            }

            this.QuestionnaireTitle = item.QuestionnaireTitle;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the featured questions.
        /// </summary>
        public Dictionary<Guid, string> FeaturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire title.
        /// </summary>
        public string QuestionnaireTitle { get; set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public SurveyStatus Status { get; private set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        #endregion
    }
}