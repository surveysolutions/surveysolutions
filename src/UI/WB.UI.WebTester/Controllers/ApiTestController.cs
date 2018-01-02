using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class ApiTestController : Controller
    {
        private readonly IDesignerWebTesterApi webTesterApi;

        public ApiTestController(IDesignerWebTesterApi webTesterApi)
        {
            this.webTesterApi = webTesterApi;
        }

        [HttpGet]
        public async Task<ActionResult> Test(Guid id)
        {
            var token = id.ToString();

            var info = await webTesterApi.GetQuestionnaireInfoAsync(token);

            var questionnaire = await webTesterApi.GetQuestionnaireAsync(token);
            
            var translations = await webTesterApi.GetTranslationsAsync(token);

            var attaches = new List<string>();

            foreach (var attach in questionnaire.Document.Attachments)
            {
                var attachment = await webTesterApi.GetAttachmentContentAsync(token, attach.ContentId);
                attaches.Add(Convert.ToBase64String(attachment.Content));
            }

            return this.View(new ApiTestModel
            {
                Id = info.Id,
                Title = questionnaire.Document.Title,
                LastUpdated = info.LastUpdateDate,
                NumOfTranslations = translations.Count(),
                Attaches = attaches
            });
        }

        public async Task<ActionResult> Index()
        {
            return this.View();
        }
    }
}