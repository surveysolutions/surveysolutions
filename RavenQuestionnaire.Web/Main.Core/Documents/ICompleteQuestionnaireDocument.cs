// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestionnaireDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The CompleteQuestionnaireDocument interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Documents
{
    using System;

    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The CompleteQuestionnaireDocument interface.
    /// </summary>
    public interface ICompleteQuestionnaireDocument : IQuestionnaireDocument, ICompleteGroup
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the question hash.
        /// </summary>
        GroupHash QuestionHash { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        Guid TemplateId { get; set; }

        #endregion
    }
}