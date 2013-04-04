using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Main.Core.Documents;
using Main.Core.View;
using NLog;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using RazorEngine;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Models;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {

        protected readonly IZipUtils ZipUtils;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        ///  <param name="zipUtils">
        /// The command service.
        /// </param>
        public SynchronizationController(IViewRepository repository, ICommandService commandService, IZipUtils zipUtils)
            : base(repository, commandService)
        {
            this.ZipUtils = zipUtils;
        }

        #endregion


        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            var document = this.ZipUtils.UnzipTemplate<IQuestionnaireDocument>(this.Request, uploadFile);
            CommandService.Execute(new ImportQuestionnaireCommand(UserHelper.CurrentUserId, document));
            return this.RedirectToAction("Index", "Questionnaire");
        }


    }
}
