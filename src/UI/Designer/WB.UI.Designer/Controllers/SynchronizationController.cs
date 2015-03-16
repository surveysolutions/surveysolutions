using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireExportService exportService;
        private readonly IStringCompressor zipUtils;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;

        public SynchronizationController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IStringCompressor zipUtils,
            IQuestionnaireExportService exportService,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.zipUtils = zipUtils;
            this.exportService = exportService;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        public FileStreamResult Export(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);

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
                var document = this.zipUtils.DecompressGZip<IQuestionnaireDocument>(uploadFile.InputStream);
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