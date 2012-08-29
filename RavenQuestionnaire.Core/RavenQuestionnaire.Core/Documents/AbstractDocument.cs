// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the AbstractDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Documents
{
    /// <summary>
    /// The abstract document.
    /// </summary>
    public abstract class AbstractDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        #endregion
    }
}