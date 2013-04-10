using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using Questionnaire.Core.Web.Helpers;
using WB.Core.Questionnaire.ImportService.Commands;

namespace Web.Supervisor.Controllers
{
    public class TemplateController : Controller
    {
        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

        public TemplateController(IGlobalInfoProvider globalInfo)
        {
            this.globalInfo = globalInfo;
        }

        #region Import from new designer

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("NewViewTestUploadFile");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            List<string> zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);
            if (zipData == null || zipData.Count == 0)
            {
                return null;
            }
            var document = DesserializeString<QuestionnaireDocument>(zipData[0]);
            NcqrsEnvironment.Get<ICommandService>()
                            .Execute(new ImportQuestionnaireCommand(globalInfo.GetCurrentUser().Id, document));


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
