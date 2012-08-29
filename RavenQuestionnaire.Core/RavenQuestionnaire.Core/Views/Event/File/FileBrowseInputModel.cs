// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileBrowseInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The file browse input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.File
{
    /// <summary>
    /// The file browse input model.
    /// </summary>
    public class FileBrowseInputModel
    {
        #region Fields

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        #endregion
    }
}