// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Assign
{
    using System;

    /// <summary>
    /// The assign survey input model.
    /// </summary>
    public class AssignSurveyInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignSurveyInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public AssignSurveyInputModel(Guid id)
        {
            this.CompleteQuestionnaireId = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; private set; }

        #endregion
    }
}