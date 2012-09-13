// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQStatusReportViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq status report view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System;

    /// <summary>
    /// The cq status report view input model.
    /// </summary>
    public class CQStatusReportViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQStatusReportViewInputModel"/> class.
        /// </summary>
        /// <param name="qId">
        /// The q id.
        /// </param>
        /// <param name="sId">
        /// The s id.
        /// </param>
        public CQStatusReportViewInputModel(Guid qId, Guid sId)
        {
            this.StatusId = sId;
            this.QuestionnaireId = qId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        public Guid StatusId { get; set; }

        #endregion
    }
}