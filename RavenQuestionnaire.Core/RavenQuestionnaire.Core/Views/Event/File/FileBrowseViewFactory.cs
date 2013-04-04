// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The file browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;

namespace RavenQuestionnaire.Core.Views.Event.File
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;

    /// <summary>
    /// The file browse view factory.
    /// </summary>
    public class FileBrowseViewFactory : IViewFactory<FileBrowseInputModel, FileBrowseView>
    {
        #region Fields

        /// <summary>
        /// The attachments.
        /// </summary>
        private readonly IDenormalizerStorage<FileDescription> attachments;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="attachments">
        /// The attachments.
        /// </param>
        public FileBrowseViewFactory(IDenormalizerStorage<FileDescription> attachments)
        {
            this.attachments = attachments;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.File.FileBrowseView.
        /// </returns>
        public FileBrowseView Load(FileBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.attachments.Query().Count();
            if (count == 0)
            {
                return new FileBrowseView(input.Page, input.PageSize, count, new FileBrowseItem[0]);
            }

            List<FileDescription> query =
                this.attachments.Query().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();

            // And enact this query
            FileBrowseItem[] items =
                query.Select(x => new FileBrowseItem( /*x.PublicKey,*/ x.Title, x.Description, x.FileName)).ToArray();

            return new FileBrowseView(input.Page, input.PageSize, count, items);
        }

        #endregion
    }
}