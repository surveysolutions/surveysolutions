// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewDataUploadFilesResult.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The view data upload files result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Models
{
    /// <summary>
    /// The view data upload files result.
    /// </summary>
    public class ViewDataUploadFilesResult
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets desc.
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets size.
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// Gets or sets thumbnail_url.
        /// </summary>
        public string thumbnail_url { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets type.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets url.
        /// </summary>
        public string url { get; set; }

        #endregion
    }
}