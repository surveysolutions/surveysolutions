// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommentDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Documents
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The comment document.
    /// </summary>
    public class CommentDocument 
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets CommentDate.
        /// </summary>
        public DateTime CommentDate { get; set; }

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets Comment.
        /// </summary>
        public string Comment { get; set; }

        #endregion
    }
}