// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using System;

    /// <summary>
    /// The questionnaire view input model.
    /// </summary>
    public class QuestionnaireViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public QuestionnaireViewInputModel(Guid id)
        {
            this.QuestionnaireId = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; private set; }

        #endregion
    }
}