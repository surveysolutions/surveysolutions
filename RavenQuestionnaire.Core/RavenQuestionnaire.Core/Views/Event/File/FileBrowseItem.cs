// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The file browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Event.File
{
    /// <summary>
    /// The file browse item.
    /// </summary>
    public class FileBrowseItem
    {
        // public Guid Id { get; set; }
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowseItem"/> class.
        /// </summary>
        public FileBrowseItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowseItem"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        public FileBrowseItem( /*Guid id,*/ string title, string description, string fileName)
        {
            // Id = id;
            this.Title = title;
            this.Description = description;
            this.FileName = fileName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The new.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.File.FileBrowseItem.
        /// </returns>
        public static FileBrowseItem New()
        {
            return new FileBrowseItem();
        }

        #endregion
    }
}