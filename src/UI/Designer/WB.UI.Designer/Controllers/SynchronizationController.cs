// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Documents;
    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using WB.Core.Questionnaire.ExportServices;
    using WB.Core.Questionnaire.ImportService.Commands;
    using WB.UI.Designer.Utilities.Compression;

    /// <summary>
    /// The synchronization controller.
    /// </summary>
    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {
        #region Fields

        /// <summary>
        /// The export service.
        /// </summary>
        protected readonly IExportService ExportService;

        /// <summary>
        /// The zip utils.
        /// </summary>
        protected readonly IZipUtils ZipUtils;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="userHelper">
        /// The user helper.
        /// </param>
        /// <param name="zipUtils">
        /// The zip utils.
        /// </param>
        /// <param name="exportService">
        /// The export service.
        /// </param>
        public SynchronizationController(
            IViewRepository repository, 
            ICommandService commandService, 
            IUserHelper userHelper, 
            IZipUtils zipUtils, 
            IExportService exportService)
            : base(repository, commandService, userHelper)
        {
            this.ZipUtils = zipUtils;
            this.ExportService = exportService;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="FileStreamResult"/>.
        /// </returns>
        [HttpGet]
        public FileStreamResult Export(Guid id)
        {
            string data = this.ExportService.GetQuestionnaireTemplate(id);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return new FileStreamResult(this.ZipUtils.Zip(data), "application/zip")
                       {
                           FileDownloadName = "template.zip"
                       };
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            uploadFile = uploadFile ?? this.Request.Files[0];

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                var document = this.ZipUtils.UnZip<IQuestionnaireDocument>(uploadFile.InputStream);
                if (document != null)
                {
                    this.CommandService.Execute(new ImportQuestionnaireCommand(this.UserHelper.CurrentUserId, document));
                    return this.RedirectToAction("Index", "Questionnaire");
                }
            }
            else
            {
                this.Error("Uploaded file is empty");    
            }
            
            return this.Import();
        }

        #endregion
    }
}