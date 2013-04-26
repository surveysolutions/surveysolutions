using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using Questionnaire.Core.Web.Helpers;
using WB.Core.Questionnaire.ImportService.Commands;

namespace Web.Supervisor.Controllers
{
    public class TemplateController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateController"/> class.
        /// </summary>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="globalInfo">
        /// The global info.
        /// </param>
        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo)
            : base(null, commandService, globalInfo)
        {
        }

        #region Import from new designer
        [Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("NewViewTestUploadFile");
        }
        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            List<string> zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);
            if (zipData == null || zipData.Count == 0)
            {
                return null;
            }
            var document = DesserializeString<QuestionnaireDocument>(zipData[0]);

            this.CommandService.Execute(new ImportQuestionnaireCommand(this.GlobalInfo.GetCurrentUser().Id, document));

            return this.RedirectToAction("Index", "Survey");
        }

        protected T DesserializeString<T>(String data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

            return JsonConvert.DeserializeObject<T>(data, settings);
        }
        #endregion
    }
}
