// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQuestionnaireDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The QuestionnaireDocument interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Documents
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The QuestionnaireDocument interface.
    /// </summary>
    public interface IQuestionnaireDocument : IGroup
    {
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the close date.
        /// </summary>
        DateTime? CloseDate { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the open date.
        /// </summary>
        DateTime? OpenDate { get; set; }

        #endregion
    }
}