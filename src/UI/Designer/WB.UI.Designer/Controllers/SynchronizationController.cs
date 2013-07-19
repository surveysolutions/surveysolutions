using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Documents;

    using Ncqrs.Commanding.ServiceModel;
    using WB.Core.Questionnaire.ExportServices;
    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Shared.Web.Membership;

    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IExportService exportService;
        private readonly IStringCompressor zipUtils;

        public SynchronizationController(
            ICommandService commandService, 
            IMembershipUserService userHelper, 
            IStringCompressor zipUtils, 
            IExportService exportService)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.zipUtils = zipUtils;
            this.exportService = exportService;
        }

        [HttpGet]
        public FileStreamResult Export(Guid id)
        {
            string data = this.exportService.GetQuestionnaireTemplate(id);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return new FileStreamResult(this.zipUtils.Compress(data), "application/zip")
                       {
                           FileDownloadName = "template.zip"
                       };
        }

        [HttpGet]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            uploadFile = uploadFile ?? this.Request.Files[0];

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                var document = this.zipUtils.Decompress<IQuestionnaireDocument>(uploadFile.InputStream);
                if (document != null)
                {
                    this.commandService.Execute(new ImportQuestionnaireCommand(this.UserHelper.WebUser.UserId, document));
                    return this.RedirectToAction("Index", "Questionnaire");
                }
            }
            else
            {
                this.Error("Uploaded file is empty");    
            }
            
            return this.Import();
        }
    }
}