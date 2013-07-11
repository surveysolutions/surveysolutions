namespace Main.Core.View.CompleteQuestionnaire.Statistics
{
    using System;

    /// <summary>
    /// The complete questionnaire statistic view input model.
    /// </summary>
    public class CompleteQuestionnaireStatisticViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireStatisticViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public CompleteQuestionnaireStatisticViewInputModel(Guid id)
        {
            this.Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        public Entities.SubEntities.QuestionScope Scope { get; set; }

        #endregion
    }
}