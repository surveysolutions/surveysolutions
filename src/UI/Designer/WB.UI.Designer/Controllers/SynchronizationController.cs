using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Questionnaire.ExportServices;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Code;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {

        protected readonly IZipUtils ZipUtils;
        protected readonly IExportService ExportService;
        #region Constructors and Destructors

        public SynchronizationController(IViewRepository repository, ICommandService commandService, IUserHelper userHelper, IZipUtils zipUtils, IExportService exportService)
            : base(repository, commandService, userHelper)
        {
            this.ZipUtils = zipUtils;
            this.ExportService = exportService;
        }

        #endregion

        [AcceptVerbs(HttpVerbs.Get)]
        public FileResult Export(Guid id)
        {
            var data = ExportService.GetQuestionnaireTemplate(id);
            if (string.IsNullOrEmpty(data))
                return null;
            return this.File(ZipUtils.ZipDate(data), "application/zip",
                             "template.zip");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            var document = this.ZipUtils.UnzipTemplate<IQuestionnaireDocument>(this.Request, uploadFile);
            if(document==null)
                return this.RedirectToAction("Index", "Error");
            CommandService.Execute(new ImportQuestionnaireCommand(UserHelper.CurrentUserId, document));
            return this.RedirectToAction("Index", "Questionnaire");
        }


    }
}
