using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class WebTesterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }

    public class ApiTestController : Controller
    {
        private readonly IDesignerWebTesterApi webTesterApi;

        public ApiTestController(IDesignerWebTesterApi webTesterApi)
        {
            this.webTesterApi = webTesterApi;
        }

        public async Task<ActionResult> Index()
        {
            var token = "6158dd07-4d64-498f-8a50-e5e9828fda23";

            var info = await webTesterApi.GetQuestionnaireInfoAsync(token);

            var questionnaire = await webTesterApi.GetQuestionnaireAsync(token);

            var translations = await webTesterApi.GetTranslationsAsync(token);

            foreach (var attach in questionnaire.Document.Attachments)
            {
                var attachment = await webTesterApi.GetAttachmentContentAsync(token, attach.ContentId);
            }

            return this.View();
        }
    }
}
