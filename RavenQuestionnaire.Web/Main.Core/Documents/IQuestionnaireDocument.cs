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
    using System.Diagnostics.CodeAnalysis;

    using Main.Core.Entities.Composite;
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

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        Guid? CreatedBy { get; set; }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parentKey">
        /// The parent key.
        /// </param>
        /// <param name="parentPropagationKey">
        /// The parent propagation key.
        /// </param>
        void Add(IComposite c, Guid? parentKey, Guid? parentPropagationKey);

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        void Remove(IComposite c);*/

        /*/// <summary>
        /// The remove.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        void Remove(Guid publicKey, Guid? propagationKey);*/

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="itemKey">
        /// The item key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation Key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        /// <param name="parentPropagationKey">
        /// The parent propagation key.
        /// </param>
        void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey);



        #endregion
    }
}