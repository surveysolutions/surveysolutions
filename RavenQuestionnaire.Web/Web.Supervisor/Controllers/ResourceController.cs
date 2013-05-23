// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the ResourceController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Services;

namespace Web.Supervisor.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Responsible for load images
    /// </summary>
    public class ResourceController : Controller
    {
        #region FieldsConstants

        private readonly IFileStorageService fileStorageService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceController"/> class.
        /// </summary>
        /// <param name="fileStorageService">
        /// The file storage service.
        /// </param>
        public ResourceController(IFileStorageService fileStorageService)
        {
            this.fileStorageService = fileStorageService;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Upload and return file
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Return file contains
        /// </returns>
        [HttpGet]
        public ActionResult Images(string id)
        {
            var fileBytes = this.fileStorageService.RetrieveFile(id).Content;
            return this.File(fileBytes, "image/png");
        }

        /// <summary>
        /// Upload and return file
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Return file contains
        /// </returns>
        [HttpGet]
        public ActionResult Thumb(string id)
        {
            var fileBytes = this.fileStorageService.RetrieveThumb(id).Content;
            return this.File(fileBytes, "image/png");
        }

        #endregion
    }
}
