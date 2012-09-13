// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireStatisticViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Statistics
{
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
        public CompleteQuestionnaireStatisticViewInputModel(string id)
        {
            this.Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; private set; }

        #endregion
    }
}