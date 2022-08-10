using System;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Authorize]
    [Route("api/hq/lookup")]
    public class HQLookupController : ControllerBase
    {
        private readonly ILookupTableService lookupTableService;
        public HQLookupController(ILookupTableService lookupTableService)
        {
            this.lookupTableService = lookupTableService;
        }

        [HttpGet]
        [Route("{id}/{tableId}")]
        public IActionResult Get(string id, string tableId)
        {
            var lookupFile = this.lookupTableService.GetLookupTableContentFile(new QuestionnaireRevision(Guid.Parse(id)), Guid.Parse(tableId));

            if (lookupFile == null) return NotFound();

            var result = JsonConvert.SerializeObject(lookupFile, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });

            return Content(result, "application/json", Encoding.UTF8);
        }
    }
}
