using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Documents;

    using Ncqrs.Commanding.ServiceModel;
    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Shared.Web;
    using WB.UI.Shared.Web.Membership;

    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IJsonExportService exportService;
        private readonly IStringCompressor zipUtils;

        public SynchronizationController(
            ICommandService commandService, 
            IMembershipUserService userHelper, 
            IStringCompressor zipUtils, 
            IJsonExportService exportService)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.zipUtils = zipUtils;
            this.exportService = exportService;
        }

        [HttpGet]
        public FileStreamResult Export(Guid id)
        {
            var templateInfo = this.exportService.GetQuestionnaireTemplate(id);

            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return null;
            }

            return new FileStreamResult(this.zipUtils.Compress(templateInfo.Source), "application/octet-stream")
                       {
                           FileDownloadName = string.Format("{0}.tmpl", templateInfo.Title.ToValidFileName())
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